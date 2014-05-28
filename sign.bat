jarsigner -signedjar aairvid\bin\arm\com.ezhang.aairvid.signed.apk -certs -sigalg SHA1withRSA -digestalg SHA1 -keystore e:\mydoc\release-key.keystore -storepass IlyZrnXl169254 -keypass IlyZrnXl169254 aairvid\bin\arm\com.ezhang.aairvid.apk googkey

jarsigner -verify aairvid\bin\arm\com.ezhang.aairvid.signed.apk 

zipalign -v 4 aairvid\bin\arm\com.ezhang.aairvid.signed.apk  aairvid\bin\arm\com.ezhang.aairvid-aligned.apk
mv aairvid\bin\arm\com.ezhang.aairvid-aligned.apk .\com.ezhang.aairvid-alignedarm.apk
jarsigner -verify .\com.ezhang.aairvid-alignedarm.apk

jarsigner -signedjar aairvid\bin\armv7\com.ezhang.aairvid.signed.apk -certs -sigalg SHA1withRSA -digestalg SHA1 -keystore e:\mydoc\release-key.keystore -storepass IlyZrnXl169254 -keypass IlyZrnXl169254 aairvid\bin\armv7\com.ezhang.aairvid.apk googkey

jarsigner -verify aairvid\bin\armv7\com.ezhang.aairvid.signed.apk 

zipalign -v 4 aairvid\bin\armv7\com.ezhang.aairvid.signed.apk  aairvid\bin\armv7\com.ezhang.aairvid-aligned.apk
mv aairvid\bin\armv7\com.ezhang.aairvid-aligned.apk .\com.ezhang.aairvid-alignedarmv7.apk
jarsigner -verify .\com.ezhang.aairvid-alignedarmv7.apk

jarsigner -signedjar aairvid\bin\x86\com.ezhang.aairvid.signed.apk -certs -sigalg SHA1withRSA -digestalg SHA1 -keystore e:\mydoc\release-key.keystore -storepass IlyZrnXl169254 -keypass IlyZrnXl169254 aairvid\bin\x86\com.ezhang.aairvid.apk googkey

jarsigner -verify aairvid\bin\x86\com.ezhang.aairvid.signed.apk 

zipalign -v 4 aairvid\bin\x86\com.ezhang.aairvid.signed.apk  aairvid\bin\x86\com.ezhang.aairvid-aligned.apk
mv aairvid\bin\x86\com.ezhang.aairvid-aligned.apk .\com.ezhang.aairvid-alignedx86.apk
jarsigner -verify .\com.ezhang.aairvid-alignedx86.apk