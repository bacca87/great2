using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Great2.Utils.Extensions
{
    public class PlainRichTextBox : Xceed.Wpf.Toolkit.RichTextBox //RichTextBox
    {

        #region Constructors
        static PlainRichTextBox()
        {
            RegisterCommandHandlers();
        }

        static void RegisterCommandHandlers()
        {
            // Register command handlers for all rich text formatting commands.
            // We disable all commands by returning false in OnCanExecute event handler,
            // thus making this control a "plain text only" RichTextBox.
            foreach (RoutedUICommand command in _formattingCommands)
            {
                CommandManager.RegisterClassCommandBinding(typeof(PlainRichTextBox),
                    new CommandBinding(command, new ExecutedRoutedEventHandler(OnFormattingCommand),
                    new CanExecuteRoutedEventHandler(OnCanExecuteFormattingCommand)));
            }

            // Command handlers for Cut, Copy and Paste commands.
            // To enforce that data can be copied or pasted from the clipboard in text format only.
            CommandManager.RegisterClassCommandBinding(typeof(PlainRichTextBox),
                new CommandBinding(ApplicationCommands.Copy, new ExecutedRoutedEventHandler(OnCopy),
                new CanExecuteRoutedEventHandler(OnCanExecuteCopy)));
            CommandManager.RegisterClassCommandBinding(typeof(PlainRichTextBox),
                new CommandBinding(ApplicationCommands.Paste, new ExecutedRoutedEventHandler(OnPaste),
                new CanExecuteRoutedEventHandler(OnCanExecutePaste)));
            CommandManager.RegisterClassCommandBinding(typeof(PlainRichTextBox),
                new CommandBinding(ApplicationCommands.Cut, new ExecutedRoutedEventHandler(OnCut),
                new CanExecuteRoutedEventHandler(OnCanExecuteCut)));
        }

        public PlainRichTextBox()
            : base()
        {
            _words = new List<Word>();
            TextChanged += TextChangedEventHandler;
        }
        #endregion

        #region Event Handlers

        public event RoutedEventHandler OnLinkClick;

        /// <summary>
        /// Event handler for all formatting commands.
        /// </summary>
        private static void OnFormattingCommand(object sender, ExecutedRoutedEventArgs e)
        {
            // Do nothing, and set command handled to true.
            e.Handled = true;
        }


        /// <summary>
        /// Event handler for ApplicationCommands.Copy command.
        /// <remarks>
        /// We want to enforce that data can be set on the clipboard 
        /// only in plain text format from this RichTextBox.
        /// </remarks>
        /// </summary>
        private static void OnCopy(object sender, ExecutedRoutedEventArgs e)
        {
            PlainRichTextBox myRichTextBox = (PlainRichTextBox)sender;
            string selectionText = myRichTextBox.Selection.Text;
            //Clipboard.SetText(selectionText);
            e.Handled = true;
        }

        /// <summary>
        /// Event handler for ApplicationCommands.Cut command.
        /// <remarks>
        /// We want to enforce that data can be set on the clipboard 
        /// only in plain text format from this RichTextBox.
        /// </remarks>
        /// </summary>
        private static void OnCut(object sender, ExecutedRoutedEventArgs e)
        {
            PlainRichTextBox myRichTextBox = (PlainRichTextBox)sender;
            string selectionText = myRichTextBox.Selection.Text;
            myRichTextBox.Selection.Text = String.Empty;
            //Clipboard.SetText(selectionText);
            e.Handled = true;
        }

        /// <summary>
        /// Event handler for ApplicationCommands.Paste command.
        /// <remarks>
        /// We want to allow paste only in plain text format.
        /// </remarks>
        /// </summary>
        private static void OnPaste(object sender, ExecutedRoutedEventArgs e)
        {
            PlainRichTextBox myRichTextBox = (PlainRichTextBox)sender;

            // Handle paste only if clipboard supports text format.
            //if (Clipboard.ContainsText())
            //{
            //    myRichTextBox.Selection.Text = Clipboard.GetText();
            //}
            e.Handled = true;
        }

        /// <summary>
        /// CanExecute event handler.
        /// </summary>
        private static void OnCanExecuteFormattingCommand(object target, CanExecuteRoutedEventArgs args)
        {
            args.CanExecute = true;
        }

        /// <summary>
        /// CanExecute event handler for ApplicationCommands.Copy.
        /// </summary>
        private static void OnCanExecuteCopy(object target, CanExecuteRoutedEventArgs args)
        {
            PlainRichTextBox myRichTextBox = (PlainRichTextBox)target;
            args.CanExecute = myRichTextBox.IsEnabled && !myRichTextBox.Selection.IsEmpty;
        }

        /// <summary>
        /// CanExecute event handler for ApplicationCommands.Cut.
        /// </summary>
        private static void OnCanExecuteCut(object target, CanExecuteRoutedEventArgs args)
        {
            PlainRichTextBox myRichTextBox = (PlainRichTextBox)target;
            args.CanExecute = myRichTextBox.IsEnabled && !myRichTextBox.IsReadOnly && !myRichTextBox.Selection.IsEmpty;
        }

        /// <summary>
        /// CanExecute event handler for ApplicationCommand.Paste.
        /// </summary>
        private static void OnCanExecutePaste(object target, CanExecuteRoutedEventArgs args)
        {
            PlainRichTextBox myRichTextBox = (PlainRichTextBox)target;
            args.CanExecute = myRichTextBox.IsEnabled && !myRichTextBox.IsReadOnly && Clipboard.ContainsText();
        }

        /// <summary>
        /// Event handler for RichTextBox.TextChanged event.
        /// </summary>
        private void TextChangedEventHandler(object sender, TextChangedEventArgs e)
        {
            FormatText();
        }

        public virtual void FormatText()
        {
            // Clear all formatting properties in the document.
            // This is necessary since a paste command could have inserted text inside or at boundaries of a keyword from dictionary.
            TextRange documentRange = new TextRange(Document.ContentStart, Document.ContentEnd);
            documentRange.ClearAllProperties();

            // Reparse the document to scan for matching words.
            TextPointer navigator = Document.ContentStart;
            while (navigator.CompareTo(Document.ContentEnd) < 0)
            {
                TextPointerContext context = navigator.GetPointerContext(LogicalDirection.Backward);
                if (context == TextPointerContext.ElementStart && navigator.Parent is Run)
                {
                    AddMatchingWordsInRun((Run)navigator.Parent);
                }
                navigator = navigator.GetNextContextPosition(LogicalDirection.Forward);
            }

            // Format words found.
            FormatWords();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Helper to apply formatting properties to matching words in the document.
        /// </summary>
        private void FormatWords()
        {
            // Applying formatting properties, triggers another TextChangedEvent. Remove event handler temporarily.
            TextChanged -= TextChangedEventHandler;

            // Add formatting for matching words.
            foreach (Word word in _words)
            {
                TextRange range = new TextRange(word.Start, word.End);
                range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));


                if (!IsMouseOver)
                {
                    //hack to avoid clr exception!
                    Hyperlink hl = new Hyperlink(range.Start, range.End);
                    hl.Tag = range.Text;
                    hl.Click += OnHyperlinkClick;
                }

            }
            _words.Clear();

            // Add TextChanged handler back.
            TextChanged += TextChangedEventHandler;
        }

        /// <summary>
        /// Scans passed Run's text, for any matching words from dictionary.
        /// </summary>
        private void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            OnLinkClick?.Invoke(sender, e);
        }

        /// <summary>
        /// Scans passed Run's text, for any matching words from dictionary.
        /// </summary>
        private void AddMatchingWordsInRun(Run run)
        {
            string runText = run.Text;

            int wordStartIndex = 0;
            int wordEndIndex = 0;
            for (int i = 0; i < runText.Length; i++)
            {
                if (Char.IsWhiteSpace(runText[i]))
                {
                    if (i > 0 && !Char.IsWhiteSpace(runText[i - 1]))
                    {
                        wordEndIndex = i - 1;
                        string wordInRun = runText.Substring(wordStartIndex, wordEndIndex - wordStartIndex + 1);

                        if (wordInRun.Contains("#"))
                        {
                            TextPointer wordStart = run.ContentStart.GetPositionAtOffset(wordStartIndex, LogicalDirection.Forward);
                            TextPointer wordEnd = run.ContentStart.GetPositionAtOffset(wordEndIndex + 1, LogicalDirection.Backward);
                            _words.Add(new Word(wordStart, wordEnd));
                        }
                    }
                    wordStartIndex = i + 1;
                }
            }

            // Check if the last word in the Run is a matching word.
            string lastWordInRun = runText.Substring(wordStartIndex, runText.Length - wordStartIndex);
            if (lastWordInRun.Contains("#"))
            {
                TextPointer wordStart = run.ContentStart.GetPositionAtOffset(wordStartIndex, LogicalDirection.Forward);
                TextPointer wordEnd = run.ContentStart.GetPositionAtOffset(runText.Length, LogicalDirection.Backward);
                _words.Add(new Word(wordStart, wordEnd));
            }
        }


        #endregion

        #region Private Types

        /// <summary>
        /// This class encapsulates a matching word by two TextPointer positions, 
        /// start and end, with forward and backward gravities respectively.
        /// </summary>
        private class Word
        {
            public Word(TextPointer wordStart, TextPointer wordEnd)
            {
                _wordStart = wordStart.GetPositionAtOffset(0, LogicalDirection.Forward);
                _wordEnd = wordEnd.GetPositionAtOffset(0, LogicalDirection.Backward);
            }

            public TextPointer Start
            {
                get { return _wordStart; }
            }

            public TextPointer End
            {
                get { return _wordEnd; }
            }

            private readonly TextPointer _wordStart;
            private readonly TextPointer _wordEnd;
        }

        #endregion

        #region Private Members

        // Static list of editing formatting commands. In the ctor we disable all these commands.
        private static readonly RoutedUICommand[] _formattingCommands = new RoutedUICommand[]
            {
                EditingCommands.ToggleBold,
                EditingCommands.ToggleItalic,
                EditingCommands.ToggleUnderline,
                EditingCommands.ToggleSubscript,
                EditingCommands.ToggleSuperscript,
                EditingCommands.IncreaseFontSize,
                EditingCommands.DecreaseFontSize,
                EditingCommands.ToggleBullets,
                EditingCommands.ToggleNumbering,
            };

        // List of matching words found in the document.
        private List<Word> _words;

        #endregion Private Members
    }

    public class IntelliRichTextBox : PlainRichTextBox
    {
        private ListBox IntellisenseList = new ListBox();

        #region Dependency Properties

        public static readonly DependencyProperty ContentAssistSourceProperty =
            DependencyProperty.Register("ContentAssistSource", typeof(IList<string>), typeof(IntelliRichTextBox), new UIPropertyMetadata(new List<string>()));

        public IList<string> ContentAssistSource
        {
            get => (IList<string>)GetValue(ContentAssistSourceProperty);
            set => SetValue(ContentAssistSourceProperty, value);
        }

        public static readonly DependencyProperty ContentAssistTriggersProperty =
            DependencyProperty.Register("ContentAssistTriggers", typeof(IList<char>), typeof(IntelliRichTextBox), new UIPropertyMetadata(new List<char>()));

        public IList<char> ContentAssistTriggers
        {
            get => (IList<char>)GetValue(ContentAssistTriggersProperty);
            set => SetValue(ContentAssistTriggersProperty, value);
        }


        public static readonly DependencyProperty LastWordProperty =
            DependencyProperty.Register("LastWord", typeof(string), typeof(IntelliRichTextBox), new UIPropertyMetadata(String.Empty));

        public string LastWord
        {
            get => (string)GetValue(LastWordProperty);
            set => SetValue(LastWordProperty, value);
        }

        public static readonly DependencyProperty CompleteWordProperty =
            DependencyProperty.Register("CompleteWord", typeof(string), typeof(IntelliRichTextBox), new UIPropertyMetadata(String.Empty));

        public string CompleteWord
        {
            get => (string)GetValue(CompleteWordProperty);
            set
            {
                SetValue(CompleteWordProperty, value);
                SetLastWord();
            }
        }

        #endregion

        public void SetCompleteWord(string appendStr = "")
        {
            var text = CaretPosition.GetTextInRun(LogicalDirection.Backward) + appendStr;
            var index = text.LastIndexOf(' ');
            if (index >= 0)
            {
                index++;
                text = text.Substring(index, text.Length - index);
            }
            CompleteWord = text;
        }

        public void SetLastWord()
        {
            var text = CompleteWord;
            var index = text.LastIndexOf(' ');
            if (index >= 0)
            {
                index++;
                text = text.Substring(index, text.Length - index);
            }
            var splitText = text.Split(new char[] { '.' });
            var t = splitText[splitText.Length - 1];
            LastWord = splitText[splitText.Length - 1];
        }

        #region .ctor

        public IntelliRichTextBox()
        {
            Loaded += new RoutedEventHandler(RichTextBoxEx_Loaded);
        }

        void RichTextBoxEx_Loaded(object sender, RoutedEventArgs e)
        {            
            if (IntellisenseList.Parent == null)
            {
                //init the assist list box
                if (Parent?.GetType() == typeof(Grid))
                    (Parent as Grid).Children.Add(IntellisenseList);

                //Create the style with Margin=0 for the Paragraph
                Style style = new Style { TargetType = typeof(Paragraph) };
                style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0)));
                //Add the style as resource.            
                Resources.Add(typeof(Paragraph), style);
            }
            IntellisenseList.SetValue(Panel.ZIndexProperty, 1000);

            if (ContentAssistTriggers.Count == 0)
                ContentAssistTriggers.Add('#');

            IntellisenseList.MaxHeight = 100;
            IntellisenseList.MinWidth = 100;
            IntellisenseList.HorizontalAlignment = HorizontalAlignment.Left;
            IntellisenseList.VerticalAlignment = VerticalAlignment.Top;
            IntellisenseList.Visibility = Visibility.Collapsed;
            IntellisenseList.BorderThickness = new Thickness(1);
            IntellisenseList.MouseDoubleClick += new MouseButtonEventHandler(AssistListBox_MouseDoubleClick);
            IntellisenseList.PreviewKeyDown += new KeyEventHandler(AssistListBox_PreviewKeyDown);
            UpdateInfo();
        }

        private void AssistListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Space)
            {
                InsertAssistWord();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                Focus();
            }
        }

        private void AssistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertAssistWord();
        }

        #endregion

        #region Insert Text

        public void InsertText(string text)
        {
            Focus();
            CaretPosition.InsertTextInRun(text);
            TextPointer pointer = CaretPosition.GetPositionAtOffset(text.Length);
            if (pointer != null)
            {
                CaretPosition = pointer;
            }
        }

        #endregion

        #region Content Assist

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key != Key.RightAlt && e.Key != Key.Back && e.Key != Key.LeftCtrl)
            {
                FilterIntellisenseWithCount();
                ShowIntellisense();
            }

            if (e.Key == Key.Escape)
            {
                IntellisenseList.Visibility = Visibility.Collapsed;
            }

            else if (e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Tab)
            {
                if (IntellisenseList.Visibility == Visibility.Visible)
                {
                    if (InsertAssistWord())
                    {
                        e.Handled = true;
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                IntellisenseList.Focus();
            }
            base.OnPreviewKeyDown(e);
        }

        private void UpdateInfo(string appendStr = "")
        {
            SetCompleteWord(appendStr);
            ResetIntellisenseLocation();
            FilterIntellisenseWithCount();
        }

        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            UpdateInfo();
        }

        private bool InsertAssistWord()
        {
            bool isInserted = false;
            if (IntellisenseList.SelectedIndex != -1)
            {
                TextPointer pointer = CaretPosition;
                var text = pointer.GetTextInRun(LogicalDirection.Backward);
                var offset = LastWord.Length * -1;
                CaretPosition = pointer.GetPositionAtOffset(offset, LogicalDirection.Backward);
                var wordToInsert = IntellisenseList.SelectedItem.ToString();
                CaretPosition.DeleteTextInRun(LastWord.Length);
                InsertText(wordToInsert);
                isInserted = true;
            }
            IntellisenseList.Visibility = Visibility.Collapsed;
            return isInserted;
        }

        private void ResetIntellisenseLocation()
        {
            var pointer = CaretPosition;
            Rect rect = pointer.GetCharacterRect(LogicalDirection.Forward);
            var text = pointer.GetTextInRun(LogicalDirection.Backward);
            var index = text.LastIndexOfAny(new char[] { '.', ' ' });
            int offset = text.Length * -1;
            if (index >= 0)
            {
                offset = (text.Length - index - 1) * -1;
            }
            pointer = pointer.GetPositionAtOffset(offset, LogicalDirection.Backward);
            rect = pointer.GetCharacterRect(LogicalDirection.Forward);
            double left = rect.X >= 0 ? rect.X : 0;
            double top = rect.Y >= 0 ? rect.Y + 20 : 0;
            left += Padding.Left;
            top += Padding.Top;
            IntellisenseList.SetCurrentValue(MarginProperty, new Thickness(left, top, 0, 0));
        }

        private int FilterIntellisenseWithCount()
        {
            if (string.IsNullOrEmpty(LastWord))
                return 0;

            IEnumerable<string> filtered = ContentAssistSource.Where(s => s.ToUpper().StartsWith(LastWord.ToUpper()));
            IntellisenseList.ItemsSource = filtered;
            IntellisenseList.SelectedIndex = 0;
            if (filtered.Count() == 0)
            {
                IntellisenseList.Visibility = Visibility.Collapsed;
            }
            return filtered.Count();
        }

        private void ShowIntellisense()
        {

            if (ContentAssistSource.Count() > 0 && FilterIntellisenseWithCount() > 0)
            {
                IntellisenseList.Visibility = Visibility.Visible;
            }
        }

        #endregion
    }

}
