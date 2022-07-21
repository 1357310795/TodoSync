using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TodoSynchronizer.Helpers;
using Wpf.Ui.Controls.Interfaces;

namespace TodoSynchronizer.Controls
{
    [TemplatePart(Name = "PART_MainBorder", Type = typeof(Border))]
    [TemplatePart(Name = "PART_HeaderBorder", Type = typeof(Border))]
    public class SideHideBar : HeaderedContentControl, IIconControl, IAppearanceControl
    {
        private const string ElementMainBorder = "PART_MainBorder";
        private const string ElementHeaderBorder = "PART_HeaderBorder";

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon),
            typeof(Wpf.Ui.Common.SymbolRegular), typeof(SideHideBar),
            new PropertyMetadata(Wpf.Ui.Common.SymbolRegular.Empty));

        public static readonly DependencyProperty IconFilledProperty = DependencyProperty.Register(nameof(IconFilled),
            typeof(bool), typeof(SideHideBar), new PropertyMetadata(false));

        public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(nameof(IconForeground),
            typeof(Brush), typeof(SideHideBar), new FrameworkPropertyMetadata(SystemColors.ControlTextBrush,
                FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty AppearanceProperty = DependencyProperty.Register(nameof(Appearance),
            typeof(Wpf.Ui.Common.ControlAppearance), typeof(SideHideBar),
            new PropertyMetadata(Wpf.Ui.Common.ControlAppearance.Primary));

        [Bindable(true), Category("Appearance")]
        public Wpf.Ui.Common.SymbolRegular Icon
        {
            get => (Wpf.Ui.Common.SymbolRegular)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public bool IconFilled
        {
            get => (bool)GetValue(IconFilledProperty);
            set => SetValue(IconFilledProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Brush IconForeground
        {
            get => (Brush)GetValue(IconForegroundProperty);
            set => SetValue(IconForegroundProperty, value);
        }

        [Bindable(true), Category("Appearance")]
        public Wpf.Ui.Common.ControlAppearance Appearance
        {
            get => (Wpf.Ui.Common.ControlAppearance)GetValue(AppearanceProperty);
            set => SetValue(AppearanceProperty, value);
        }

        public static readonly DependencyProperty TitleBackgroundProperty = DependencyProperty.Register(nameof(TitleBackground),
        typeof(Brush), typeof(SideHideBar),
        new PropertyMetadata(Border.BorderBrushProperty.DefaultMetadata.DefaultValue));

        [Bindable(true), Category("Appearance")]
        public Brush TitleBackground
        {
            get => (Brush)GetValue(TitleBackgroundProperty);
            set => SetValue(TitleBackgroundProperty, value);
        }

        public static readonly DependencyProperty TitleForegroundProperty = DependencyProperty.Register(nameof(TitleForeground),
        typeof(Brush), typeof(SideHideBar),
        new PropertyMetadata(Border.BorderBrushProperty.DefaultMetadata.DefaultValue));

        [Bindable(true), Category("Appearance")]
        public Brush TitleForeground
        {
            get => (Brush)GetValue(TitleForegroundProperty);
            set => SetValue(TitleForegroundProperty, value);
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(SideHideBar), new PropertyMetadata(null));

        Border mainBorder, headerBorder;

        public SideHideBar()
        {
            this.Loaded += SideHideBar_Loaded;
        }

        private void SideHideBar_Loaded(object sender, RoutedEventArgs e)
        {
            //Canvas.SetLeft(mainBorder, -mainBorder.ActualWidth);
            Main_Animation(true);
            Header_Animation(false);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            mainBorder = GetTemplateChild(ElementMainBorder) as Border;
            headerBorder = GetTemplateChild(ElementHeaderBorder) as Border;

            if (mainBorder != null)
            {
                headerBorder.MouseEnter += HeaderBorder_MouseEnter;
                this.MouseLeave += MainBorder_MouseLeave;
            }
        }


        private void MainBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Main_Animation(true);
            Header_Animation(false);
        }

        private void HeaderBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Main_Animation(false);
            Header_Animation(true);
        }

        private void Header_Animation(bool hide)
        {
            //var tt = new TranslateTransform();
            //headerBorder.LayoutTransform = tt;
            if (hide)
            {
                var ani = AnimationHelper.CubicBezierDoubleAnimation(
                    TimeSpan.FromSeconds(0.8),
                    -headerBorder.ActualHeight,
                    "0,.64,.22,1");
                headerBorder.BeginAnimation(Canvas.LeftProperty, ani);
            }
            else
            {
                var ani = AnimationHelper.CubicBezierDoubleAnimation(
                    TimeSpan.FromSeconds(0.8),
                    0,
                    "0,.64,.22,1");
                headerBorder.BeginAnimation(Canvas.LeftProperty, ani);
            }
        }

        private void Main_Animation(bool hide)
        {
            //var tt = TransformHelper.CreateTranslateTransform(mainBorder.LayoutTransform);
            //if (tt is null)
            //{
            //    tt = new TranslateTransform();
            //    mainBorder.LayoutTransform = tt;
            //}
            if (hide)
            {
                var ani = AnimationHelper.CubicBezierDoubleAnimation(
                    TimeSpan.FromSeconds(0.5),
                    -mainBorder.ActualWidth,
                    "0,.64,.22,1");
                mainBorder.BeginAnimation(Canvas.LeftProperty, ani);
            }
            else
            {
                var ani = AnimationHelper.CubicBezierDoubleAnimation(
                    TimeSpan.FromSeconds(0.5),
                    0,
                    "0,.64,.22,1");
                mainBorder.BeginAnimation(Canvas.LeftProperty, ani);
            }
        }
    }
}
