# MissinKit

## License
The code is released under the [MIT License](https://opensource.org/licenses/MIT).

## What's Inside?
#### iOS Native Pluralization
As Xamarin's current `NSString.LocalizedFormat()` method is useless,
there's no way to use iOS native pluralization. To turn this very important feature on,
there's the `NSStringUtility.LocalizedFormat()` method to replace Xamarin's counterpart:
```csharp
NSStringUtility.LocalizedFormat(NSBundle.MainBundle.LocalizedNSString("%d file(s) remaining"), 2)
```
If key is found, it formats the string accordinig to the .stringsdict file. Otherwise, it falls back and prints "2 file(s) remaining".

Note that the `NSStringUtility.LocalizedFormat()` method must be used in pair with the `NSBundle.LocalizedNSString()` extension method
as the latter returns a special instance of `NSString` (`__NSLocalizedString`) with the data from the .stringsdict file.

WARNING! The `NSStringUtility.LocalizedFormat()` method does not work on 64-bit simulators. There's no known workaround.

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

#### Machine Epsilon
As on some ARM devices both `float.Epsilon` and `double.Epsilon` equate to zero, the constants `MachineEpsilon.Single` and `MachineEpsilon.Double` are recommended to be used instead.