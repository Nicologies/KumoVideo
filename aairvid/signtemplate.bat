jarsigner -signedjar bin\Release\com.ezhang.aairvid.signed.apk -certs -sigalg SHA1withRSA -digestalg SHA1 -keystore Your_key.keystore -storepass YourStorePassWordHere -keypass YourkeyPassHere bin\Release\com.ezhang.aairvid.apk googkey

jarsigner -verify bin\Release\com.ezhang.aairvid.signed.apk 

zipalign -v 4 bin\Release\com.ezhang.aairvid.signed.apk  bin\Release\com.ezhang.aairvid-aligned.apk
mv bin\Release\com.ezhang.aairvid-aligned.apk .\com.ezhang.aairvid-aligned.apk