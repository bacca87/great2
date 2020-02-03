using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Markup;
using Xceed.Wpf.Toolkit;

namespace Great2.Utils.Extensions
{
    public class CustomFormatter : ITextFormatter
    {
        public string GetText(FlowDocument document)
        {

            return new TextRange(document.ContentStart, document.ContentEnd).Text;
        }

        public void SetText(FlowDocument document, string text)
        {
            try
            {
                //if the text is null/empty clear the contents of the RTB. If you were to pass a null/empty string
                //to the TextRange.Load method an exception would occur.
                if (String.IsNullOrEmpty(text))
                {
                    document.Blocks.Clear();
                }
                else
                {
                    document.Blocks.Clear();
                    Paragraph par = new Paragraph();


                    //search for tags
                    string[] words = text.Split(' ');

                    if (words?.Length > 0)
                    {

                        foreach (string w in words)
                        {
                            Run r = new Run(w);

                            if (w.StartsWith("#"))
                            {
                                Hyperlink hlink = new Hyperlink(r);
                                hlink.IsEnabled = true;
                                par.Inlines.Add(hlink);
                                hlink.NavigateUri = new Uri(GetUrl(w));

                            }
                            else
                            {

                                par.Inlines.Add(r);
                            }

                            par.Inlines.Add(new Run(" "));
                                
                        }

                        document.Blocks.Add(par);
                    }



                    //TextRange tr = new TextRange(document.ContentStart, document.ContentEnd);
                    //using (MemoryStream ms = new MemoryStream(Encoding.ASCII.GetBytes(text)))
                    //{
                    //    tr.Load(ms, DataFormats.Xaml);
                    //}
                }
            }
            catch
            {
                // throw new InvalidDataException("Data provided is not in the correct RTF format.");
            }
        }


        static string GetUrl(string key)
        {
            return string.Format(@"https://www.google.com/#q={0}", key);
        }
    }
}
