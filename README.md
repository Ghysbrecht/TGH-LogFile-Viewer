# Log Viewer Tool

### What is this?

This is an application where you can view DataMiner log files in chronological order. Extra features include filtering, parsing and analyzing.

### What do I need?

These are the requirements to use this application and it's features:
- .NET Framework
- Java
- Elasticsearch (Running locally or on a  server)

### How do I use it?

A guide on how to use this can be viewed via this link: https://drive.google.com/open?id=1ptK4PUJR-U8FOWpXf2wTja--nq0uybxq

### Why did you make this?

This is my bachelor's thesis. I am attending my last year at Vives Brugge (Campus Spoorwegstraat)

### Changelog

##### V0.1.0 :
-	First release

##### V0.1.1 :
-	BUG – Fixed crash when there was no DB connection.
-	BUG – Fixed a crash when an index was given that did not exist.
-	BUG – Fixed error when filtering on a message that contains a CR or \

##### V0.1.2 :
-	BUG – Fixed a bug that made it impossible to get data without setting time bounds.
-	FEATURE – The FROM and TO boxes are now clearable when doubleclicked.

##### V0.1.3:
-	FEATURE – Parse button on the top left.
-	FEATURE – Get data button is now async.
-	FEATURE – After parsing, there is an option to use the server settings in the log viewer.
-	FEATURE – Data grid headers are now fixed when scrolling.
-	BUG – Indexes are now default ‘tghlogstasher’.
-	BUG – Icons are now visible.
