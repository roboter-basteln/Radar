using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace radar
{
    class Objects
    {
        public Ellipse ellipse;


        public bool goingLeft = true;
        public bool goingRight = false;

        public void initialization()
        {
            var br = new SolidColorBrush
            {
                Color = Color.FromRgb(127, 255, 0)
            };

            var she = new DropShadowEffect
            {
                Color = Color.FromScRgb(255, 0, 2.3f, 0),
                ShadowDepth = 0,
                Direction = 0,
                BlurRadius = 20
            };

            ellipse = new Ellipse
            {
                StrokeThickness = 4,
                Stroke = br,
                Fill = br,
                Effect = she
            };
        }




    }
}
