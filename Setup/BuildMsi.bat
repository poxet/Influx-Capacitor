for /f "delims=" %%i in ('"C:\Program Files (x86)\Windows Kits\8.1\bin\x64\uuidgen.exe"') do set output=%%i
"C:\Program Files (x86)\WiX Toolset v3.9\bin\candle.exe" Influx-Capacitor.wsx -dEnvironment="Release" -dVersion="1.0.8" -dId="%output%"
"C:\Program Files (x86)\WiX Toolset v3.9\bin\light.exe" -ext WixUIExtension -cultures:en-us Influx-Capacitor.wixobj