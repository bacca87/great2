using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace Great2.Utils.Extensions
{
    public class RichTextBoxEx : Xceed.Wpf.Toolkit.RichTextBox
    {
        public bool AutoAddWhiteSpaceAfterTriggered
        {
            get => (bool)GetValue(AutoAddWhiteSpaceAfterTriggeredProperty);
            set => SetValue(AutoAddWhiteSpaceAfterTriggeredProperty, value);
        }

        // Using a DependencyProperty as the backing store for AutoAddWhiteSpaceAfterTriggered.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AutoAddWhiteSpaceAfterTriggeredProperty =
               DependencyProperty.Register("AutoAddWhiteSpaceAfterTriggered", typeof(bool), typeof(RichTextBoxEx), new UIPropertyMetadata(true));

        public IList<string> ContentAssistSource
        {
            get => (IList<string>)GetValue(ContentAssistSourceProperty);
            set => SetValue(ContentAssistSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for ContentAssistSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentAssistSourceProperty =
               DependencyProperty.Register("ContentAssistSource", typeof(IList<string>), typeof(RichTextBoxEx), new UIPropertyMetadata(new List<string>()));

        #region Content Assist
        private bool IsAssistKeyPressed = false;
        private System.Text.StringBuilder sbLastWords = new System.Text.StringBuilder();
        private ListBox AssistListBox = new ListBox();

        protected override void OnPreviewKeyDown(System.Windows.Input.KeyEventArgs e)
        {
            if (!IsAssistKeyPressed && e.Key != Key.Back)
            {
                base.OnPreviewKeyDown(e);
                return;
            }

            ResetAssistListBoxLocation();

            if (e.Key == Key.Back)
            {
                if (sbLastWords.Length > 0)
                {
                    sbLastWords.Remove(sbLastWords.Length - 1, 1);
                    FilterAssistBoxItemsSource();
                }
                else
                {
                    IsAssistKeyPressed = false;
                    sbLastWords.Clear();
                    AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
                }
            }

            //enter key pressed, insert the first item to richtextbox
            if ((e.Key == Key.Enter || e.Key == Key.Space || e.Key == Key.Tab))
            {
                var existing = ContentAssistSource.SingleOrDefault(x => x == sbLastWords.ToString());
                if (existing == null)
                {
                    ContentAssistSource.Add(sbLastWords.ToString());
                    IEnumerable<string> temp = ContentAssistSource.Where(s => s.ToUpper().StartsWith(sbLastWords.ToString().ToUpper()));
                    AssistListBox.ItemsSource = temp;
                }

                AssistListBox.SelectedIndex = 0;
                if (InsertAssistWord())
                {
                    e.Handled = true;
                }
            }

            if (e.Key == Key.Down)
            {
                AssistListBox.Focus();
            }



            base.OnPreviewKeyDown(e);
        }

        private void FilterAssistBoxItemsSource()
        {
            IEnumerable<string> temp = ContentAssistSource.Where(s => s.ToUpper().StartsWith(sbLastWords.ToString().ToUpper()));
            AssistListBox.ItemsSource = temp;
            AssistListBox.SelectedIndex = 0;
            if (temp.Count() == 0)
            {
                AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                AssistListBox.Visibility = System.Windows.Visibility.Visible;
            }
        }

        protected override void OnTextInput(System.Windows.Input.TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (IsAssistKeyPressed == false && e.Text.Length == 1)
            {
                if (char.Parse(e.Text) == '#')
                {
                    ResetAssistListBoxLocation();
                    IsAssistKeyPressed = true;
                    FilterAssistBoxItemsSource();
                    return;
                }
            }

            if (IsAssistKeyPressed)
            {
                sbLastWords.Append(e.Text);
                FilterAssistBoxItemsSource();
            }
        }

        private void ResetAssistListBoxLocation()
        {
            Rect rect = this.CaretPosition.GetCharacterRect(LogicalDirection.Forward);
            double left = rect.X >= 20 ? rect.X : 20;
            double top = rect.Y >= 20 ? rect.Y + 20 : 20;
            left += this.Padding.Left;
            top += this.Padding.Top;
            AssistListBox.SetCurrentValue(ListBox.MarginProperty, new Thickness(left, top, 0, 0));
        }

        private bool InsertAssistWord()
        {
            bool isInserted = false;
            if (AssistListBox.SelectedIndex != -1)
            {
                string selectedString = AssistListBox.SelectedItem.ToString().Remove(0, sbLastWords.Length);
                if (AutoAddWhiteSpaceAfterTriggered)
                {
                    selectedString += " ";
                }

                this.Text = this.Text.Replace("\r\n", string.Empty);
                this.Text += selectedString;
                this.CaretPosition = CaretPosition.DocumentEnd;
                isInserted = true;
            }

            AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
            sbLastWords.Clear();
            IsAssistKeyPressed = false;
            return isInserted;
        }

        void AssistListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //if Enter\Tab\Space key is pressed, insert current selected item to richtextbox
            if (e.Key == Key.Enter || e.Key == Key.Tab || e.Key == Key.Space)
            {
                InsertAssistWord();
                e.Handled = true;
            }
            else if (e.Key == Key.Back)
            {
                //Baskspace key is pressed, set focus to richtext box
                if (sbLastWords.Length >= 1)
                {
                    sbLastWords.Remove(sbLastWords.Length - 1, 1);
                }
                this.Focus();
            }
        }

        void AssistListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            InsertAssistWord();
        }
        #endregion

        public RichTextBoxEx()
        {
            this.Loaded += new RoutedEventHandler(RichTextBoxEx_Loaded);
            void RichTextBoxEx_Loaded(object sender, RoutedEventArgs e)
            {
                //init the assist list box
                if (this.Parent?.GetType() != typeof(Grid))
                {
                    throw new Exception("this control must be put in Grid control");
                }

                Grid parent = (this.Parent as Grid);
                if (!parent.Children.Contains(AssistListBox)) parent.Children.Add(AssistListBox);
                AssistListBox.MaxHeight = 100;
                AssistListBox.MinWidth = 100;
                AssistListBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                AssistListBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                AssistListBox.Visibility = System.Windows.Visibility.Collapsed;
                AssistListBox.MouseDoubleClick += new MouseButtonEventHandler(AssistListBox_MouseDoubleClick);
                AssistListBox.PreviewKeyDown += new KeyEventHandler(AssistListBox_PreviewKeyDown);
            }
        }
    }

}
