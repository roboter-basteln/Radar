using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Windows.Media.Effects;

namespace radar
{
    public partial class MainWindow : Window
    {
        readonly SerialPort mySerialPort;
        string data;
        float distance, distanceNormal = 536;
        int angle;

        float[,] border = new float[6, 181];
        float[] intruderBorder = new float[181];

        bool borderIsComplete;
        bool isDrawed;
        bool test;

        Line line;
        Line line2;
        Polyline bor;

        Objects[] obj;
        Ellipse[] ell;
        Ellipse[] intr;

        public MainWindow()
        {
            InitializeComponent();
            var ib = new ImageBrush
            {
                ImageSource = new BitmapImage(new Uri(@"radar.png", UriKind.Relative))
            };
            canvas.Background = ib;

            obj = new Objects[181];
            ell = new Ellipse[181];
            intr = new Ellipse[181];

            lineSettings1();
            lineSetting2();
            borderSetting();
            intruderSetting();

            mySerialPort = new SerialPort(texbox1.Text);
            mySerialPort.BaudRate = 9600;
            mySerialPort.DataReceived += DataReceivedHandler;

            for (var i = 1; i < 181; i++)
            {
                obj[i] = new Objects();
                ell[i] = new Ellipse();

                obj[i].initialization();
                ell[i] = obj[i].ellipse;
                ell[i].Width = 12;
                ell[i].Height = 12;
            }


            dis1.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 + "cm";
            dis2.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 * 2 + "cm";
            dis3.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 * 3 + "cm";
            dis4.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 * 4 + "cm";

            line.Y2 = 536f - Math.Sin(0) * distanceNormal;
            line.X2 = Math.Cos(0) * distanceNormal + 536;

            line2.Y2 = 536f - Math.Sin(0) * distanceNormal;
            line2.X2 = Math.Cos(0) * distanceNormal + 536;


            canvas.Children.Add(line);
            canvas.Children.Add(line2);
        }


        private void StartCOM(object sender, RoutedEventArgs e)
        {
            try
            {
                mySerialPort.Open();
            }
            catch
            {
            }

            try
            {
                if (mySerialPort.IsOpen)
                {
                    mySerialPort.WriteLine(textbox2.Text);
                    mySerialPort.WriteLine("-1");

                }
            }
            catch { }
        }

        private void stopCOM(object sender, RoutedEventArgs e)
        {
            if (mySerialPort.IsOpen)
            {
                mySerialPort.WriteLine("-2");
                distance = 0;
                angle = 0;

                canvas.Children.Remove(bor);
                for (var i = 0; i < 181; i++)
                {
                    canvas.Children.Remove(ell[i]);
                    canvas.Children.Remove(intr[i]);
                }
            }
        }


        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var indata = mySerialPort.ReadLine();
                data = indata;
            }
            catch
            {

            }

            Dispatcher.Invoke(() =>
            {
                printDataAndConvertDataToInt();

                if (angle == 180)
                {
                    test = true;
                }
                else if (angle == 0)
                {
                    test = false;
                }

                if (test == true)
                {
                    angle += 10;
                }
                if (angle <= 180)
                {
                    if (!checkBox.IsChecked.Value)
                    {
                        draw();
                        objectDraw();
                    }
                    else
                    {
                        if (borderIsComplete == false)
                        {
                            scan_status.Content = "finding border ..";
                            scanBorder();
                            draw();
                            scan_status.Content = "finding border ...";
                        }
                        else
                        {
                            if (isDrawed == false)
                            {
                                scan_status.Content = "done";
                                drawBorder();
                                isDrawed = true;
                            }
                            else
                            {
                                draw();
                                drawIntruder();
                            }

                        }
                    }
                }
            });

        }

        private void datachanget(object sender, KeyEventArgs e)
        {
            try
            {
                dis1.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 + "cm";
                dis2.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 * 2 + "cm";
                dis3.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 * 3 + "cm";
                dis4.Content = float.Parse(distanceGlobal.Text, System.Globalization.CultureInfo.InvariantCulture) / 4 * 4 + "cm";
            }
            catch { }
        }

        private void printDataAndConvertDataToInt()
        {
            var values = data.Split('*');
            values[0] = values[0].Replace(".", ",");
            label1.Content = values[0] + "cm";
            label2.Content = values[1] + "°";

            distance = float.Parse(values[0]);
            int.TryParse(values[1], out angle);
        }

        private void draw()
        {
            canvas.Children.Remove(line);
            canvas.Children.Remove(line2);
            var angleLocal = angle * Convert.ToSingle(Math.PI) / 180;

            line.Y2 = 536f - Math.Sin(angleLocal) * distanceNormal;
            line.X2 = Math.Cos(angleLocal) * distanceNormal + 536;

            line2.Y2 = 536f - Math.Sin(angleLocal) * distanceNormal;
            line2.X2 = Math.Cos(angleLocal) * distanceNormal + 536;


            canvas.Children.Add(line);
            canvas.Children.Add(line2);

        }

        private void objectDraw()
        {
            var maxDistance = float.Parse(dis4.Content.ToString().Replace("cm", ""));


            if (distance <= maxDistance)
            {
                var ratio = 536 / maxDistance;

                var localAngle = angle * Convert.ToSingle(Math.PI) / 180;

                if (canvas.Children.Contains(ell[angle]))
                {
                    canvas.Children.Remove(ell[angle]);
                }

                try
                {
                    Canvas.SetTop(ell[angle], 536f - Math.Sin(localAngle) * distance * ratio);
                    Canvas.SetLeft(ell[angle], 536 + Math.Cos(localAngle) * distance * ratio);

                    canvas.Children.Add(ell[angle]);
                }
                catch
                {
                }
            }
            else
            {
                canvas.Children.Remove(ell[angle]);
            }


        }


        private void scanBorder()
        {
            var maxDistance = float.Parse(dis4.Content.ToString().Replace("cm", ""));

            if (border[0, 180].ToString() == "0")
            {
                if (distance > maxDistance)
                {
                    border[0, angle] = maxDistance;
                }
                else
                {
                    border[0, angle] = distance;
                }
            }
            else if (border[1, 0].ToString() == "0")
            {
                if (distance > maxDistance)
                {
                    border[1, angle] = maxDistance;
                }
                else
                {
                    border[1, angle] = distance;
                }
            }
            else if (border[2, 180].ToString() == "0")
            {
                if (distance > maxDistance)
                {
                    border[2, angle] = maxDistance;
                }
                else
                {
                    border[2, angle] = distance;
                }
            }
            else if (border[3, 0].ToString() == "0")
            {
                if (distance > maxDistance)
                {
                    border[3, angle] = maxDistance;
                }
                else
                {
                    border[3, angle] = distance;
                }
            }
            else if (border[4, 180].ToString() == "0")
            {
                if (distance > maxDistance)
                {
                    border[4, angle] = maxDistance;
                }
                else
                {
                    border[4, angle] = distance;
                }
            }
            else if (border[5, 0].ToString() == "0")
            {
                if (distance > maxDistance)
                {
                    border[5, angle] = maxDistance;
                }
                else
                {
                    border[5, angle] = distance;
                }
            }
            else
            {
                borderIsComplete = true;
            }
        }

        private void drawIntruder()
        {
            short confirmed = 0;
            if (distance < border[0, angle] - 5)
            {
                confirmed++;
            }
            if (distance < border[1, angle] - 5)
            {
                confirmed++;
            }
            if (distance < border[2, angle] - 5)
            {
                confirmed++;
            }
            if (distance < border[3, angle] - 5)
            {
                confirmed++;
            }
            if (distance < border[4, angle] - 5)
            {
                confirmed++;
            }
            if (distance < border[5, angle] - 5)
            {
                confirmed++;
            }


            if (confirmed == 6)
            {
                var maxDistance = float.Parse(dis4.Content.ToString().Replace("cm", ""));

                var ratio = 536 / maxDistance;

                var localAngle = angle * Convert.ToSingle(Math.PI) / 180;

                if (canvas.Children.Contains(intr[angle]))
                {
                    canvas.Children.Remove(intr[angle]);
                }

                try
                {
                    Canvas.SetTop(intr[angle], 536f - Math.Sin(localAngle) * distance * ratio);
                    Canvas.SetLeft(intr[angle], 536 + Math.Cos(localAngle) * distance * ratio);

                    canvas.Children.Add(intr[angle]);
                }
                catch { }
            }
            else if (confirmed == 0)
            {
                if (canvas.Children.Contains(intr[angle]))
                {
                    canvas.Children.Remove(intr[angle]);
                }
            }


        }

        private void drawBorder()
        {
            var po = new Point[181];
            var polygonPoints = new PointCollection();

            var maxDistance = float.Parse(dis4.Content.ToString().Replace("cm", ""));
            var ratio = 536 / maxDistance;

            for (var i = 0; i < 181; i++)
            {
                var angleLocal = i * Convert.ToSingle(Math.PI) / 180;
                float avgdis = 0;

                for (var a = 0; a < 6; a++)
                {
                    avgdis += border[a, i];
                }

                avgdis /= 6;

                intruderBorder[i] = avgdis;
                po[i] = new Point(Convert.ToSingle(Math.Cos(angleLocal) * avgdis * ratio + 536f), Convert.ToSingle(536f - Math.Sin(angleLocal) * avgdis * ratio));
                polygonPoints.Add(po[i]);
            }
            bor.Points = polygonPoints;
            canvas.Children.Add(bor);
        }

        private void intruderSetting()
        {
            var br = new SolidColorBrush
            {
                Color = Color.FromRgb(235, 12, 12)
            };

            for (var i = 0; i < 181; i++)
            {
                intr[i] = new Ellipse
                {
                    Width = 12,
                    Height = 12,
                    StrokeThickness = 4,
                    Stroke = br,
                    Fill = br
                };
            }
        }

        private void borderSetting()
        {
            var she = new DropShadowEffect
            {
                Color = Color.FromScRgb(255, 0, 2.3f, 0),
                ShadowDepth = 0,
                Direction = 0,
                BlurRadius = 20
            };

            var Brush = new SolidColorBrush
            {
                Color = Color.FromRgb(127, 255, 0)
            };

            bor = new Polyline
            {
                Effect = she,
                Stroke = Brush,
                StrokeThickness = 5
            };
        }

        private void lineSettings1()
        {
            line = new Line
            {
                X1 = 536,
                Y1 = 531
            };

            var she = new DropShadowEffect
            {
                Color = Color.FromScRgb(255, 0, 2.3f, 0),
                ShadowDepth = 0,
                Direction = 0,
                BlurRadius = 20
            };

            var Brush = new SolidColorBrush
            {
                Color = Color.FromRgb(127, 255, 0)
            };

            line.Effect = she;
            line.Stroke = Brush;
            line.StrokeThickness = 5;

        }

        private void lineSetting2()
        {
            line2 = new Line
            {
                X1 = 536,
                Y1 = 531
            };

            var she2 = new DropShadowEffect
            {
                Color = Color.FromScRgb(255, 0, 2, 0),
                ShadowDepth = 0,
                Direction = 0,
                BlurRadius = 80
            };

            var Brush2 = new SolidColorBrush
            {
                Color = Color.FromRgb(127, 255, 0)
            };

            line2.Effect = she2;
            line2.Stroke = Brush2;
            line2.StrokeThickness = 5;
        }

    }
}
