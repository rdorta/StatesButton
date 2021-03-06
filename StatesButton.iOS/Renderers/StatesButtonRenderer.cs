﻿using System;
using System.Reflection;
using System.Threading.Tasks;
using Foundation;
using StatesButton.Forms;
using StatesButton.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using StatesButton.Shared;
using CoreGraphics;

[assembly: ExportRenderer(typeof(StatesButtonControl), typeof(StatesButtonRenderer))]
namespace StatesButton.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class StatesButtonRenderer : ButtonRenderer
    {
        public static void Init()
        {
            var hack = DateTime.Now;
        }

        UIButton _byPassButton;

        public StatesButtonRenderer()
        {
        }

        public StatesButtonControl BaseElement => Element as StatesButtonControl;

        protected async override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            if (_byPassButton == null)
            {
                _byPassButton = new UIButton(UIButtonType.Custom);
                _byPassButton.Frame = this.Frame;
                SetNativeControl(_byPassButton);
                base.Control.TouchUpInside += byPassButton_TouchUpInside;
                base.Control.TouchUpInside += byPassButton_TouchDown;

                SetField(this, "_buttonTextColorDefaultNormal", base.Control.TitleColor(UIControlState.Normal));
                SetField(this, "_buttonTextColorDefaultHighlighted", base.Control.TitleColor(UIControlState.Highlighted));
                SetField(this, "_buttonTextColorDefaultDisabled", base.Control.TitleColor(UIControlState.Disabled));

                InvokeMethod(this, "UpdateText", null);
                InvokeMethod(this, "UpdateFont", null);
                InvokeMethod(this, "UpdateBorder", null);
                InvokeMethod(this, "UpdateImage", null);
                InvokeMethod(this, "UpdateTextColor", null);
            }

            if (e.NewElement != null)
            {
                Control.ShowsTouchWhenHighlighted = false;
                Control.AdjustsImageWhenHighlighted = false;
                await SetNormalImageResource();
                await SetDisableImageResource();
                await SetPressImageResource();
                var statesButton = e.NewElement as StatesButtonControl;
                if (statesButton.BackgroundColor != Color.Default && statesButton.PressedBackgroundColor != Color.Default && statesButton.DisableBackgroundColor != Color.Default)
                {
                    Control.ShowsTouchWhenHighlighted = false;
                    SetNormalColorResource();
                    SetDisableColorResource();
                    SetPressColorResource();
                }
            }
        }

        protected async override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            if (e.PropertyName == StatesButtonControl.NormalImageProperty.PropertyName)
            {
                await SetNormalImageResource();
            }
            else if (e.PropertyName == StatesButtonControl.DisableImageProperty.PropertyName)
            {
                await SetDisableImageResource();
            }
            else if (e.PropertyName == StatesButtonControl.PressedImageProperty.PropertyName)
            {
                await SetPressImageResource();
            }
            else if (e.PropertyName == VisualElement.BackgroundColorProperty.PropertyName)
            {
                SetNormalColorResource();
            }
            else if (e.PropertyName == StatesButtonControl.DisableBackgroundColorProperty.PropertyName)
            {
                SetDisableColorResource();
            }
            else if (e.PropertyName == StatesButtonControl.PressedBackgroundColorProperty.PropertyName)
            {
                SetPressColorResource();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (base.Control != null)
                {
                    base.Control.TouchUpInside -= byPassButton_TouchUpInside;
                    base.Control.TouchDown -= byPassButton_TouchDown;
                }
            }
            base.Dispose(disposing);
        }

        void byPassButton_TouchUpInside(object sender, EventArgs e)
        {
            InvokeMethod(this, "OnButtonTouchUpInside", sender, e);
        }

        void byPassButton_TouchDown(object sender, EventArgs e)
        {
            InvokeMethod(this, "OnButtonTouchDown", sender, e);
        }

        #region Color Impl

        void SetNormalColorResource()
        {
            UIImage source = null;
            if (BaseElement.BackgroundColor != Color.Default)
            {
                source = ImageFromColor(BaseElement.BackgroundColor.ToUIColor());
            }
            Control.SetBackgroundImage(source, UIControlState.Normal);
        }

        void SetDisableColorResource()
        {
            UIImage source = null;
            if (BaseElement.BackgroundColor != Color.Default)
            {
                source = ImageFromColor(BaseElement.DisableBackgroundColor.ToUIColor());
            }
            Control.SetBackgroundImage(source, UIControlState.Disabled);
        }

        void SetPressColorResource()
        {
            UIImage source = null;
            if (BaseElement.BackgroundColor != Color.Default)
            {
                source = ImageFromColor(BaseElement.PressedBackgroundColor.ToUIColor());
            }
            Control.ShowsTouchWhenHighlighted = false;
            Control.SetBackgroundImage(source, UIControlState.Selected);
            Control.SetBackgroundImage(source, UIControlState.Highlighted);
        }

        UIImage ImageFromColor(UIColor color)
        {
            CGRect rect = new CGRect(0.0f, 0.0f, 1.0f, 1.0f);
            UIGraphics.BeginImageContext(rect.Size);
            CGContext context = UIGraphics.GetCurrentContext();
            context.SetFillColor(color.CGColor);
            context.FillRect(rect);
            UIImage image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();
            return image;
        }

        #endregion

        #region Image Impl

        async Task SetNormalImageResource()
        {
            UIImage source = null;
            if (BaseElement.NormalImage != null)
            {
                var handler = BaseElement.NormalImage.GetHandler();
                source = await handler.LoadImageAsync(BaseElement.NormalImage);
            }
            Control.SetBackgroundImage(source, UIControlState.Normal);
        }

        async Task SetDisableImageResource()
        {
            UIImage source = null;
            if (BaseElement.DisableImage != null)
            {
                var handler = BaseElement.DisableImage.GetHandler();
                source = await handler.LoadImageAsync(BaseElement.DisableImage);
            }
            Control.SetBackgroundImage(source, UIControlState.Disabled);
        }

        async Task SetPressImageResource()
        {
            UIImage source = null;
            if (BaseElement.PressedImage != null)
            {
                var handler = BaseElement.PressedImage.GetHandler();
                source = await handler.LoadImageAsync(BaseElement.PressedImage);
                base.Control.ShowsTouchWhenHighlighted = false;
            }

            if (source == null)
            {
                base.Control.ShowsTouchWhenHighlighted = true;
            }
            Control.SetBackgroundImage(source, UIControlState.Selected);
            Control.SetBackgroundImage(source, UIControlState.Highlighted);
        }

        #endregion

        public object InvokeMethod(object target, string methodName, params object[] args)
        {
            Type t = target.GetType();
            MethodInfo mi = null;

            while (t != null)
            {
                mi = t.GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (mi != null) break;

                t = t.BaseType;
            }

            if (mi == null)
            {
                throw new Exception(string.Format("Method '{0}' not found in type hierarchy.", methodName));
            }

            return mi.Invoke(target, args);
        }

        public void SetField(object target, string fieldName, object value)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target), "The assignment target cannot be null.");
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                throw new ArgumentException(nameof(fieldName), "The field name cannot be null or empty.");
            }

            Type t = target.GetType();
            FieldInfo fi = null;

            while (t != null)
            {
                fi = t.GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

                if (fi != null) break;

                t = t.BaseType;
            }

            if (fi == null)
            {
                throw new Exception(string.Format("Field '{0}' not found in type hierarchy.", fieldName));
            }

            fi.SetValue(target, value);
        }
    }
}