# MissinKit
[![NuGet](https://img.shields.io/nuget/v/MissinKit.svg)](https://www.nuget.org/packages/MissinKit)

## License
The code is released under the [MIT License](https://opensource.org/licenses/MIT).

## What's Inside?
#### iOS Native Pluralization
As Xamarin's current `NSString.LocalizedFormat()` method is useless,
there's no way to use iOS native pluralization. To turn this very important feature on,
there's the `NSStringUtility.LocalizedFormat()` method to replace Xamarin's counterpart:
```csharp
NSStringUtility.LocalizedFormat(NSBundle.MainBundle.GetLocalizedString("%d file(s) remaining"), 2)
```
If key is found, it formats the string accordinig to the .stringsdict file. Otherwise, it falls back and prints "2 file(s) remaining".

WARNING! The `NSStringUtility.LocalizedFormat()` method does not work on 64-bit simulators.

Instead of using the `NSStringUtility.LocalizedFormat()` or `NSBundle.GetLocalizedString()` methods directly,
use the convenient extension methods which are more than enough for the vast majority of cases:
```csharp
// instead of
str = NSBundle.MainBundle.GetLocalizedString("key");
// use
str = "key".Localize();

// instead of
str = string.format(NSBundle.MainBundle.GetLocalizedString("key"), 2, "text");
// use
str = "key".Localize(2, "text");

// instead of
str = NSStringUtility.LocalizedFormat(NSBundle.MainBundle.GetLocalizedString("key"), 3);
// use
str = "key".Localize(3);
```
These methods work with both .strings and .stringsdict files.

#### Network Reachability
The `Reachability` class is a port of Apple's Reachability sample app to monitor the network state of an iOS device.
```csharp
var r = new Reachability("apple.com");
r.ReachabilityChanged += (s, e) => Debug.WriteLine($"Network Status: {r.Status}");
```

#### Date Utilities
There are two extension methods providing a quick way to convert `DateTime` to `NSDate` and vice-versa.
```csharp
NSDate nsdate = DateTime.Now.ToNSDate();
DateTime datetime = nsdate.ToDateTime();
```
Not a big deal, but convenient.

#### Changing Current Locale on the Fly
Since iOS doesn't provide an easy way to change `NSLocale.CurrentLocale` programmatically,
there's a utility to help with that.
```csharp
var date = new DateTime(2018, 1, 1).ToNSDate();
var formatter = new NSDateFormatter() { DateFormat = "EEEE" };

L10n.OverrideCurrentLocale("es_MX");

formatter.Locale = NSLocale.CurrentLocale;
Debug.WriteLine(formatter.StringFor(date)); // outputs 'lunes'

L10n.RestoreCurrentLocale();

formatter.Locale = NSLocale.CurrentLocale;
Debug.WriteLine(formatter.StringFor(date)); // outputs 'Monday'
```
Overriding or restoring the current locale triggers `NSCurrentLocaleDidChangeNotification`.

#### iOS 11 Fallback
Since iOS 11 introduced some breaking changes to UIView layout handling,
there are a few fallback extension methods to reduce boilerplate code required to handle those changes.

The `SafeAreaLayoutGuide()` method returns the `UIVIew.SafeAreaLayoutGuide` for iOS 11 and later
and falls back to a layout guide based on the frame rectangle of the current view for older systems.
```csharp
var insets = view.SafeAreaLayoutGuide();
```
The `SafeAreaLayoutGuide()` method uses the frame layout guide as a fallback.
The frame layout guide is a layout guide anchors of which simply return its owning view's anchors.
The frame layout guide is created using the `FrameLayoutGuide()` extension method
which is especially helpful for the scenarios like this:
```csharp
var isIos11OrLater = UIDevice.CurrentDevice.CheckSystemVersion(11, 0);
var widthAnchor = isIos11OrLater ? view.SafeAreaLayoutGuide.WidthAnchor : view.WidthAnchor;
var heightAnchor = isIos11OrLater ? view.SafeAreaLayoutGuide.HeightAnchor : view.HeightAnchor;
// more checks here...
```
To avoid checking for all anchors of the view you need, simple use the frame layout guide:
```csharp
var layoutGuide = isIos11OrLater ? view.SafeAreaLayoutGuide : view.FrameLayoutGuide();
```
This is exactly what the `SafeAreaLayoutGuide()` method does so you don't even need to check the system version.
But you can use the `FrameLayoutGuide()` method as a fallback for other layout guides introduced in iOS 11 if needed.

The initial call of the `FrameLayoutGuide()` method adds the returned layout guide to the view's layout guides,
so all subsequent calls return the already existing instance instead of creating a new one every time.

Note that the `FrameLayoutGuide()` method returns the `FrameLayoutGuide` property for `UIScrollView`.

The `SafeAreaInsets()` method returns the `UIVIew.SafeAreaInsets` for iOS 11 and later
and falls back to UIEdgeInsets.Zero for older systems.
```csharp
var insets = view.SafeAreaInsets();
```
The `AdjustedContentInset()` method returns the `UIScrollView.AdjustedContentInset` for iOS 11 and later
and falls back to `UIScrollView.ContentInset` for older systems.
```csharp
var insets = scrollView.AdjustedContentInset();
```

#### Update Watcher
This is a simple utility for a quick check for app updates in App Store.
```csharp
var updateWatcher = new UpdateWatcher(3); // Checks every 3 days
var updateAvailable = await updateWatcher.CheckForUpdateAsync();

if (updateAvailable)
    // Notify user
```

#### Machine Epsilon
As on some ARM devices both `float.Epsilon` and `double.Epsilon` equate to zero, the constants `MachineEpsilon.Single` and `MachineEpsilon.Double` are recommended to be used instead.
