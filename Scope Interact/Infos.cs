using ShiyukiUtils.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ScopeInteract
{
    public static class Infos
    {
        public static MainWindow _main;
        public static SettingsManager sf;
        static Storyboard sb;

        public static void Init(MainWindow main)
        {
            _main = main;
            sf = new SettingsManager(AppDomain.CurrentDomain.BaseDirectory + "Settings.cfg");
            isInit = true;
            //
            sb = new Storyboard();
            sb.Children.Add(addDoubleAnimation(_main.err, TimeSpan.FromMilliseconds(200), 0, 1, new PropertyPath(UIElement.OpacityProperty)));
            sb.Children.Add(addDoubleAnimation(_main.err_pb, new TimeSpan(0, 0, 5), 0, 100, new PropertyPath(ProgressBar.ValueProperty)));
            sb.Completed += (sender, e) =>
            {
                Storyboard sb2 = new Storyboard();
                sb2.Children.Add(addDoubleAnimation(_main.err, TimeSpan.FromMilliseconds(200), 1, 0, new PropertyPath(UIElement.OpacityProperty)));
                sb2.Completed += (sendere, ee) =>
                {
                    _main.err.Visibility = Visibility.Hidden;
                };
                sb2.Begin();
            };
        }

        public static void addLog(string log)
        {
            if (_main.logs.Text.Length > 5000) _main.logs.Text = "";
            if (log.Contains("Error") || (bool)_main.logs_e.IsChecked) _main.logs.Text = log + "\n" + _main.logs.Text;
            /*logs.SelectionStart = logs.Text.Length;
            logs.ScrollToEnd();*/
        }

        public static bool isInit = false;

        public static void newErr(Exception erri, string errname)
        {
            string s = "";
            if (erri != null)
            {
                s = erri.Message + " (" + erri.Source + ")\nStacktrace :\n" + erri.StackTrace;
            }
            else
            {
                s = errname;
            }
            string text = "";
            if (erri != null)
            {
                text = s;
                if (errname != null)
                {
                    text = errname;
                }
            }
            else
            {
                text = errname;
            }
            try
            {
                _main.err_text.Content = text;
                _main.err_name.Content = (erri != null ? erri.Message : errname);
                _main.err_pb.Value = 0;
                _main.err.Visibility = Visibility.Visible;
                sb.Stop();
                sb.Begin();
            }
            catch { MessageBox.Show(text, "ScopeInteract Error", MessageBoxButton.OK, MessageBoxImage.Error); }
            addLog("Error : " + s);
        }

        public static void newErrLog(Exception erri, string errname)
        {
            string s = "";
            if (erri != null)
            {
                s = (errname != null ? errname + "\n" : null) + erri.Message + " (" + erri.Source + ")\nStacktrace :\n" + erri.StackTrace;
            }
            else
            {
                s = errname;
            }
            addLog("Error : " + s);
        }

        public static DoubleAnimation addDoubleAnimation(DependencyObject el, TimeSpan time, double? from, double? to, PropertyPath property)
        {
            DoubleAnimation da = new DoubleAnimation();
            da.From = from;
            da.To = to;
            da.Duration = new Duration(time);
            Storyboard.SetTarget(da, el);
            Storyboard.SetTargetProperty(da, property);
            return da;
        }

        public static double strToDouble(string str)
        {
            str = str.Replace(",", ".");
            return Convert.ToDouble(str, System.Globalization.CultureInfo.InvariantCulture);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
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
    }
}
