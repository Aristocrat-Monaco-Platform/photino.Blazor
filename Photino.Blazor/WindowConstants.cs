namespace Photino.Blazor
{
    using System.Drawing;

    /// <summary>
    ///     Photino Window Constants
    /// </summary>
    public static class WindowConstants
    {
        public const int DefaultWidth = 1000;
        public const int DefaultHeight = 900;
        public const int DefaultLeft = 450;
        public const int DefaultTop = 100;

        public static readonly Size DefaultSize = new Size(DefaultWidth, DefaultHeight);
        public static readonly Point DefaultLocation = new Point(DefaultLeft, DefaultTop);
    }
}
