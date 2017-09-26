
using System;
using System.IO;

namespace clipr.Usage
{
    public class DisplayWidth : IComparable<DisplayWidth>
    {
        public static readonly DisplayWidth Automatic = new AutomaticDisplayWidth();

        public static readonly DisplayWidth Default = new DisplayWidth(80);

        public virtual int Width { get; private set; }

        private DisplayWidth() { }

        public DisplayWidth(int width)
        {
            Width = width;
        }

        public int CompareTo(DisplayWidth other)
        {
            if(other == null)
            {
                return 1;
            }
            return Width - other.Width;
        }

        public static implicit operator DisplayWidth(int width)
        {
            return new DisplayWidth(width);
        }

        public override string ToString()
        {
            return "<DisplayWidth " + Width + ">";
        }

        private class AutomaticDisplayWidth : DisplayWidth
        {
            public override int Width
            {
                get
                {
                    try
                    {
                        return Console.WindowWidth;
                    }
                    catch (IOException)
                    {
                        return Default.Width; // Console.IsOutputRedirected doesn't exist until .Net 4.5
                    }
                }
            }
        }
    }
}
