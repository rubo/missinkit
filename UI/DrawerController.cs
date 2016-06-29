// Copyright 2016 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using UIKit;

namespace MissinKit.UI
{
    [Register("DrawerController")]
    public class DrawerController : UIViewController
    {
        #region Fields
        private static readonly nfloat MinVelocity = 900;

        private UIViewController _contentViewController;
        private nfloat _drawerWidth = 270;
        private bool _ignorePan;
        private bool _isShadowDropped;
        private UIView _overlayView;
        private UIPanGestureRecognizer _panGestureRecognizer;
        private nfloat _panOriginX;
        private UIColor _shadowColor = UIColor.Black;
        private float _shadowOpacity = .5F;
        private nfloat _shadowRadius = 5;
        private UIViewController _sideViewController;
        private double _slidingDuration = CATransaction.AnimationDuration;
        private UITapGestureRecognizer _tapGestureRecognizer;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the DrawerViewController class.
        /// </summary>
        /// <param name="contentViewController">
        /// The view controller for the main content.
        /// </param>
        /// <param name="sideViewController">
        /// The view controller for the drawer.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="contentViewController"/> is null.
        /// </exception>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="sideViewController"/> is null.
        /// </exception>
        public DrawerController(UIViewController contentViewController, UIViewController sideViewController)
        {
            if (contentViewController == null)
                throw new ArgumentNullException(nameof(contentViewController));

            if (sideViewController == null)
                throw new ArgumentNullException(nameof(sideViewController));

            _contentViewController = contentViewController;
            _sideViewController = sideViewController;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Closes the drawer.
        /// </summary>
        /// <param name="animate">
        /// <c>true</c> to use sliding animation; otherwise, <c>false</c>.
        /// </param>
        /// <param name="completionHandler">
        /// Completion action to execute after closing finishes.
        /// </param>
        public virtual void CloseDrawer(bool animate, Action completionHandler)
        {
            if (!IsDrawerOpen)
                return;

            if (animate)
                UIView.Animate(SlidingDuration, 0, UIViewAnimationOptions.CurveEaseInOut, OnCloseAnimating, () => OnCloseAnimated(completionHandler));
            else
            {
                OnCloseAnimating();
                OnCloseAnimated(completionHandler);
            }
        }

        /// <summary>
        /// Opens the drawer.
        /// </summary>
        /// <param name="animate">
        /// <c>true</c> to use sliding animation; otherwise, <c>false</c>.
        /// </param>
        /// <param name="completionHandler">
        /// Completion action to execute after opening finishes.
        /// </param>
        public virtual void OpenDrawer(bool animate, Action completionHandler)
        {
            if (IsDrawerOpen)
                return;

            SetShadowHidden(false);
            SetStatusBarHidden(true);

            ContentViewController.View.EndEditing(true);

            if (animate)
                UIView.Animate(SlidingDuration, 0, UIViewAnimationOptions.CurveEaseInOut, OnOpenAnimating, () => OnOpenAnimated(completionHandler));
            else
            {
                OnOpenAnimating();
                OnOpenAnimated(completionHandler);
            }
        }

        /// <summary>
        /// Toggles the drawer open or closed.
        /// </summary>
        public virtual void ToggleDrawer()
        {
            if (IsDrawerOpen)
                CloseDrawer(true, null);
            else
                OpenDrawer(true, null);
        }
        #endregion

        #region View Lifecycle Methods
        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            var frame = View.Bounds;

            if (DrawerPosition == DrawerPosition.Left)
                frame.X = 0;
            else if (DrawerPosition == DrawerPosition.Right)
                frame.X = frame.Width - DrawerWidth;

            frame.Width = DrawerWidth;

            SideViewController.View.Frame = frame;

            if (IsDrawerOpen)
            {
                SetShadowHidden(false, true);

                _overlayView.Frame = new CGRect(CGPoint.Empty, ContentViewController.View.Frame.Size);
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _overlayView = new UIView();

            _panGestureRecognizer = new UIPanGestureRecognizer
            {
                MaximumNumberOfTouches = 1,
                MinimumNumberOfTouches = 1,
                ShouldReceiveTouch = (recognizer, touch) => true
            };
            _panGestureRecognizer.AddTarget(OnPanGestureRecognized);

            _tapGestureRecognizer = new UITapGestureRecognizer { NumberOfTapsRequired = 1 };
            _tapGestureRecognizer.AddTarget(OnTapGestureRecognized);

            SideViewController = _sideViewController;
            ContentViewController = _contentViewController;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            View.SetNeedsLayout();
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets the main content view controller.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="value"/> is null.
        /// </exception>
        public virtual UIViewController ContentViewController
        {
            get { return _contentViewController; }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                var location = CGPoint.Empty;

                if (_contentViewController?.View != null)
                {
                    location = _contentViewController.View.Frame.Location;

                    _contentViewController.View.RemoveFromSuperview();
                }

                _contentViewController = value;

                var frame = _contentViewController.View.Frame;

                frame.Location = location;

                _contentViewController.View.Bounds = View.Bounds;
                _contentViewController.View.Frame = frame;
                _contentViewController.View.Layer.AnchorPoint = new CGPoint(.5F, .5F);
                _contentViewController.View.AddGestureRecognizer(_panGestureRecognizer);

                View.AddSubview(_contentViewController.View);

                AddChildViewController(_contentViewController);

                if (IsDrawerOpen)
                {
                    _contentViewController.View.AddGestureRecognizer(_tapGestureRecognizer);

                    SetShadowHidden(false, true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the drawer position.
        /// </summary>
        public virtual DrawerPosition DrawerPosition { get; set; } = DrawerPosition.Right;

        /// <summary>
        /// Gets or sets the drawer width.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative.
        /// </exception>
        public virtual nfloat DrawerWidth
        {
            get { return _drawerWidth; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Non-negative number required.");

                _drawerWidth = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to drop a shadow over the drawer.
        /// </summary>
        public virtual bool DropShadow { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether to hide the status bar when drawer is open.
        /// </summary>
        public virtual bool HideStatusBar { get; set; }

        /// <summary>
        /// Gets a value indicating whether the drawer is open.
        /// </summary>
        public virtual bool IsDrawerOpen { get; private set; }

        /// <summary>
        /// Gets or sets the radius of the shadow over the drawer.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative.
        /// </exception>
        public virtual nfloat ShadowRadius
        {
            get
            {
                return _shadowRadius;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Non-negative number required.");

                _shadowRadius = value;
            }
        }

        /// <summary>
        /// Gets or sets the opacity of the shadow over the drawer.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="value"/> is less than 0 or greater than 1.
        /// </exception>
        public virtual float ShadowOpacity
        {
            get
            {
                return _shadowOpacity;
            }
            set
            {
                if (value < 0 || value > 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Opacity must be between 0 and 1.");

                _shadowOpacity = value;
            }
        }

        /// <summary>
        /// Gets or sets the color of the shadow over the drawer.
        /// </summary>
        /// <exception cref="T:System.ArgumentNullException">
        /// <paramref name="value"/> is null.
        /// </exception>
        public virtual UIColor ShadowColor
        {
            get
            {
                return _shadowColor;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                _shadowColor = value;
            }
        }

        /// <summary>
        /// Gets the side view controller.
        /// </summary>
        public virtual UIViewController SideViewController
        {
            get { return _sideViewController; }
            private set
            {
                _sideViewController = value;

                View.AddSubview(SideViewController.View);

                AddChildViewController(_sideViewController);
            }
        }

        /// <summary>
        /// Gets or sets the animation duration when opening or closing the drawer.
        /// </summary>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// <paramref name="value"/> is negative.
        /// </exception>
        public virtual double SlidingDuration
        {
            get { return _slidingDuration; }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "Non-negative number required.");

                _slidingDuration = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether drawer can be opened or closed by swiping.
        /// </summary>
        public virtual bool UseGestures { get; set; } = true;
        #endregion

        #region Private Methods
        private void OnCloseAnimated(Action completionHandler)
        {
            _overlayView.RemoveFromSuperview();

            ContentViewController.View.RemoveGestureRecognizer(_tapGestureRecognizer);

            IsDrawerOpen = false;

            SetShadowHidden(true);
            SetStatusBarHidden(false);

            completionHandler?.Invoke();
        }
        private void OnCloseAnimating()
        {
            var view = _contentViewController.View;

            view.Frame = new CGRect(0, 0, view.Frame.Width, view.Frame.Height);
        }

        private void OnOpenAnimated(Action completionHandler)
        {
            _overlayView.Frame = new CGRect(CGPoint.Empty, ContentViewController.View.Frame.Size);

            ContentViewController.View.AddSubview(_overlayView);
            ContentViewController.View.AddGestureRecognizer(_tapGestureRecognizer);

            IsDrawerOpen = true;

            completionHandler?.Invoke();
        }

        private void OnOpenAnimating()
        {
            var view = _contentViewController.View;
            var x = DrawerPosition == DrawerPosition.Left ? DrawerWidth : -DrawerWidth;

            view.Frame = new CGRect(x, view.Frame.Y, view.Frame.Width, view.Frame.Height);
        }

        private void OnPanBegan()
        {
            var view = _contentViewController.View;
            var lX = _panGestureRecognizer.LocationInView(view).X;
            var delta = DrawerWidth / 2;

            _ignorePan = DrawerPosition == DrawerPosition.Left ? lX > delta : lX < view.Bounds.Width - delta;
            _panOriginX = view.Frame.X;
        }

        private void OnPanChanged()
        {
            var view = _contentViewController.View;
            var tX = _panGestureRecognizer.TranslationInView(view).X;

            if (DrawerPosition == DrawerPosition.Left && (tX > 0 && !IsDrawerOpen || tX < 0 && IsDrawerOpen))
            {
                if (tX > DrawerWidth || tX < -DrawerWidth && IsDrawerOpen)
                    tX = DrawerWidth;

                if (_panOriginX + tX <= DrawerWidth)
                    view.Frame = new CGRect(_panOriginX + tX, view.Frame.Y, view.Frame.Width, view.Frame.Height);

                SetShadowHidden(false);
                SetStatusBarHidden(true);
            }
            else if (DrawerPosition == DrawerPosition.Right && (tX < 0 && !IsDrawerOpen || tX > 0 && IsDrawerOpen))
            {
                if (tX < -DrawerWidth)
                    tX = -DrawerWidth;
                else if (tX > DrawerWidth)
                    tX = DrawerWidth;

                if (_panOriginX + tX <= 0)
                    view.Frame = new CGRect(_panOriginX + tX, view.Frame.Y, view.Frame.Width, view.Frame.Height);

                SetShadowHidden(false);
                SetStatusBarHidden(true);
            }
        }

        private void OnPanEnded()
        {
            var view = _contentViewController.View;
            var tX = _panGestureRecognizer.TranslationInView(view).X;
            var vX = _panGestureRecognizer.VelocityInView(view).X;

            if (IsDrawerOpen)
            {
                if (DrawerPosition == DrawerPosition.Left && tX < 0 && (view.Frame.X < view.Frame.Width / 2 || vX <= -MinVelocity) ||
                    DrawerPosition == DrawerPosition.Right && tX > 0 && (view.Frame.X > -view.Frame.Width / 2 || vX >= MinVelocity))
                    CloseDrawer(true, null);
                else
                    UIView.Animate(SlidingDuration, 0, UIViewAnimationOptions.CurveEaseInOut, OnOpenAnimating, null);
            }
            else
            {
                if (DrawerPosition == DrawerPosition.Left && (tX >= DrawerWidth / 2 || vX >= MinVelocity) ||
                    DrawerPosition == DrawerPosition.Right && (tX <= -DrawerWidth / 2 || vX <= -MinVelocity))
                    OpenDrawer(true, null);
                else
                    UIView.Animate(SlidingDuration, 0, UIViewAnimationOptions.CurveEaseInOut,
                        OnCloseAnimating, () =>
                        {
                            SetShadowHidden(true);
                            SetStatusBarHidden(false);
                        });
            }
        }

        private void OnPanGestureRecognized()
        {
            if (!UseGestures)
                return;

            switch (_panGestureRecognizer.State)
            {
                case UIGestureRecognizerState.Began:
                {
                    OnPanBegan();
                    break;
                }
                case UIGestureRecognizerState.Changed:
                {
                    if (!_ignorePan)
                        OnPanChanged();

                    break;
                }
                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Ended:
                {
                    if (!_ignorePan)
                        OnPanEnded();

                    break;
                }
            }
        }

        private void OnTapGestureRecognized()
        {
            CloseDrawer(true, null);
        }

        private void SetStatusBarHidden(bool hidden)
        {
            if (HideStatusBar)
                UIApplication.SharedApplication.StatusBarHidden = hidden;
        }

        private void SetShadowHidden(bool hidden, bool enforceRender = false)
        {
            if (!DropShadow)
                return;

            var layer = ContentViewController.View.Layer;

            if (hidden)
            {
                if (_isShadowDropped || enforceRender)
                {
                    layer.ShadowColor = UIColor.Clear.CGColor;
                    layer.ShadowOpacity = 0;
                    layer.ShadowRadius = 0;

                    _isShadowDropped = false;
                }
            }
            else if (!_isShadowDropped || enforceRender)
            {
                layer.ShadowColor = ShadowColor.CGColor;
                layer.ShadowOpacity = ShadowOpacity;
                layer.ShadowPath = UIBezierPath.FromRect(ContentViewController.View.Bounds).CGPath;
                layer.ShadowRadius = ShadowRadius;

                _isShadowDropped = true;
            }
        }
        #endregion
    }

    public enum DrawerPosition
    {
        Left,
        Right
    }
}
