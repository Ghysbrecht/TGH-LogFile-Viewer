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

##### V0.1.4:
-	FEATURE – Settings are now saved when closing the application
-	FEATURE – Added infinite scrolling (Automatically loads new data while scrolling)
-	FEATURE – Status bar
-	FEATURE – You can go back to previous filters with the back button or back mouse button
-	FEATURE – Time is shown while hovering over bars in the graph
-	FEATURE – Files can be excluded from all
-	IMPROVEMENT – Replaced folder selector with better version
-	IMPROVEMENT – Index error now includes the index naming rules
-	IMPROVEMENT – Milliseconds are now shown in datagrid
-	IMPROVEMENT – Page number can be edited in top right  
-	BUG – Main datagrid now expands to the full available width
-	OTHER – Renamed to Skyline Log Viewer
-	OTHER – Guide is linked on the info page

##### V0.1.5:
-	FEATURE – Graph can now be docked under the datagrid
-	FEATURE – Added filtering on graph by clicking on a bar or selecting a timeframe (Click and drag)
-	FEATURE – Time is now also saved in history
-	FEATURE – Index window where unnecessary indices can be removed
-	FEATURE – You can now change the theme
-	IMPROVEMENT – Index name is checked before starting local parsing script
-	OTHER – Autoscroll can be disabled in settings
