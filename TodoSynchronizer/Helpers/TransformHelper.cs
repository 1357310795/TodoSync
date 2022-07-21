using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TodoSynchronizer.Helpers
{
    public static class TransformHelper
    {
        public static ScaleTransform FindScaleTransform(Transform hayStack)
        {
            if (hayStack is ScaleTransform)
                return (ScaleTransform)hayStack;

            if (hayStack is TransformGroup)
            {
                TransformGroup group = hayStack as TransformGroup;

                foreach (var child in group.Children)
                {
                    if (child is ScaleTransform)
                        return (ScaleTransform)child;
                }
            }

            return null/* TODO Change to default(_) if this is not a reference type */;
        }
        public static RotateTransform FindRotateTransform(Transform hayStack)
        {
            if (hayStack is RotateTransform)
                return (RotateTransform)hayStack;

            if (hayStack is TransformGroup)
            {
                TransformGroup group = hayStack as TransformGroup;

                foreach (var child in group.Children)
                {
                    if (child is RotateTransform)
                        return (RotateTransform)child;
                }
            }
            return null/* TODO Change to default(_) if this is not a reference type */;
        }
        public static TranslateTransform FindTranslateTransform(Transform hayStack)
        {
            if (hayStack is TranslateTransform)
                return (TranslateTransform)hayStack;

            if (hayStack is TransformGroup)
            {
                TransformGroup group = hayStack as TransformGroup;

                foreach (var child in group.Children)
                {
                    if (child is TranslateTransform)
                        return (TranslateTransform)child;
                }
            }
            return null;
        }

        internal static TranslateTransform CreateTranslateTransform(Transform hayStack)
        {
            if (hayStack is TranslateTransform)
                return (TranslateTransform)hayStack;

            if (hayStack is TransformGroup)
            {
                TransformGroup group = hayStack as TransformGroup;

                foreach (var child in group.Children)
                {
                    if (child is TranslateTransform)
                        return (TranslateTransform)child;
                }

                var tt = new TranslateTransform();
                group.Children.Add(tt);
                return tt;
            }
            return null;
        }
    }
}
