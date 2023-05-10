using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ScopeInteract;

namespace ShiyukiUtils.Class
{
    /// <summary>
    /// Add a small + of your text :)
    /// HOW TO USE : "§#(color code)§(text)" or "§-(b : bold | u : underline | i : italic)§(text)" or "§#(color)§-(font style)§"
    /// You can combinate all font style.
    /// Use "§-r§" to reset color and font style.
    /// Use TextUtils.Text to change the text in conceptor.
    /// </summary>
    public static class TextUtils
    {
        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }

        /// <summary>
        /// If you use the syntax, you can call the method to change all labels on your main grid.
        /// Not recommended for performance reason.
        /// </summary>
        /// <param name="main">Grid where there are labels to change. GRID ONLY.</param>
        public static void changeAllLabel(Grid g)
        {
            foreach (UIElement l in FindVisualChildren<UIElement>(g))
            {
                if (l.GetType() == typeof(Label))
                {
                    Label la = (Label)l;
                    la.Content = changeContentLabel(la);
                }
            }
        }

        private static bool yes = true;

        private static void ch(Label label)
        {
            string main = "§";
            string text = (string)label.Content;
            WrapPanel wp = new WrapPanel();
            List<string> word = text.Split(new string[] { main }, StringSplitOptions.None).ToList();
            string mainfont = "-";
            SolidColorBrush scb = (SolidColorBrush)label.Foreground;
            FontWeight fw = label.FontWeight;
            FontStyle fs = label.FontStyle;
            TextDecorationCollection td = new TextDecorationCollection();
            bool link = false;
            foreach (string w in word)
            {
                if (yes && w.StartsWith("#") && word.IndexOf(w) != 0 || yes && w.StartsWith(mainfont) && word.IndexOf(w) != 0)
                {
                    if (w.StartsWith("#"))
                    {
                        scb = new SolidColorBrush();
                        try
                        {
                            scb.Color = (Color)ColorConverter.ConvertFromString(w);
                        }
                        catch (Exception e)
                        {
                            Infos.newErr(e, "Invalid color code ! Text : " + w);
                        }
                    }
                    if (w.StartsWith(mainfont) && !w.StartsWith(mainfont + " "))
                    {
                        switch (w)
                        {
                            case "-b":
                                fw = FontWeights.Bold;
                                break;

                            case "-i":
                                fs = FontStyles.Italic;
                                break;

                            case "-u":
                                td = TextDecorations.Underline;
                                break;

                            case "-s":
                                td = TextDecorations.Strikethrough;
                                break;

                            case "-r":
                                td = new TextDecorationCollection();
                                scb = (SolidColorBrush)label.Foreground;
                                fw = label.FontWeight;
                                fs = label.FontStyle;
                                link = false;
                                break;

                            case "-l":
                                try {
                                    scb = new SolidColorBrush();
                                    scb.Color = (Color)ColorConverter.ConvertFromString("#FF00b0f4");
                                    link = true;
                                }
                                catch (Exception e) { Infos.newErr(e, "Error to create a link. " + e.Message + "\nStackTrace : \n" + e.StackTrace); }
                                break;

                            default:
                                return;
                        }
                    }
                }
                else
                {
                    TextBlock lab = new TextBlock();
                    lab.Text = w;
                    lab.TextWrapping = TextWrapping.Wrap;
                    if (w.EndsWith("\\"))
                    {
                        List<char> anti = w.ToList();
                        string a = "";
                        foreach (char str in anti)
                        {
                            if (str.Equals('\\')) a = a + str.ToString().Replace("\\", "") + "§";
                            else a = a + str;
                        }
                        lab.Text = a;
                    }
                    lab.Foreground = scb;
                    lab.FontStyle = fs;
                    lab.TextDecorations = td;
                    lab.FontWeight = fw;
                    if (link)
                    {
                        lab.Cursor = Cursors.Hand;
                        lab.MouseDown += (sender, e) =>
                        {
                            Process.Start(w);
                        };
                        lab.MouseEnter += (sender, e) =>
                        {
                            lab.TextDecorations = new TextDecorationCollection();
                            lab.TextDecorations = TextDecorations.Underline;
                        };
                        lab.MouseLeave += (sender, e) =>
                        {
                            lab.TextDecorations = new TextDecorationCollection();
                        };
                    }
                    if (link) link = false;
                    wp.Children.Add(lab);
                }
                if (w.EndsWith("\\")) yes = false;
                else yes = true;
            }
            label.Content = wp;
        }

        /// <summary>
        /// If you use the syntax on the label content, you can use this method to change your label style.
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public static object changeContentLabel(Label label)
        {
            try
            {
                if (label.Content.GetType() == typeof(string))
                {
                    string text = (string)label.Content;
                    label.Tag = label.Content;
                    StackPanel wp = new StackPanel();
                    if (((string)label.Content).Contains("\n") || ((string)label.Content).Contains("\r\n"))
                    {
                        List<string> bl = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
                        foreach (string wo in bl)
                        {
                            Label tb = new Label();
                            tb.Foreground = (SolidColorBrush)GetColor(label);
                            tb.FontWeight = label.FontWeight;
                            tb.FontStyle = label.FontStyle;
                            TextDecorationCollection td = new TextDecorationCollection();
                            tb.Content = wo;
                            tb.Padding = new Thickness(0);
                            if (((string)tb.Content).Contains("§")) ch(tb);
                            wp.Children.Add(tb);
                        }
                    }
                    if (wp.Children.Count > 1) return wp;
                    else
                    {
                        if (((string)label.Content).Contains("§")) ch(label);
                        return label.Content;
                    }
                }
                return label.Content;
            }
            catch
            {
                return label.Content;
            }
        }

        /// <summary>
        /// If you would like to change your label style in your code, you can use this method.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text"></param>
        public static void changeLabel(Label label, string text)
        {
            label.Content = text;
            label.Content = changeContentLabel(label);
        }

        public static string getContentLabelToString(Label lab)
        {
            return (string)lab.Tag;
        }

        public static DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text", typeof(string), typeof(TextUtils), new PropertyMetadata("", OnTextChanged));

        public static void SetText(DependencyObject element, string value)
            => element.SetValue(TextProperty, value);

        public static string GetText(DependencyObject element)
            => (string)element.GetValue(TextProperty);

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if(d.GetType() == typeof(Label))
            {
                Label lab = (Label)d;
                changeLabel(lab, GetText(d));
            }
        }

        public static DependencyProperty ColorProperty = DependencyProperty.RegisterAttached(
            "Color", typeof(Brush), typeof(TextUtils), new PropertyMetadata(Brushes.White));

        public static void SetColor(DependencyObject element, Brush value)
            => element.SetValue(ColorProperty, value);

        public static Brush GetColor(DependencyObject element)
            => (Brush)element.GetValue(ColorProperty);
    }
}
//By ShiyukiNeko v2.0