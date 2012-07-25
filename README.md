aspConf-cqrs
============

Code repo for my aspconf 2012 talk on CQRS &amp; Event Sourcing 
[ http://channel9.msdn.com/Events/aspConf/aspConf/CQRS-with-ASP-NET-MVC-A-Year-On]

Steps to Run
------------

Create a new Sql Server database (the demo was created for Sql 2008) called SqlEventStore. 
Run the ES.sql script from the root folder for the new database. 
The only place you need to change the connection string (if required) is in the web.config of the AccountManager project.
Run the app.

If you wish to try out the testing "framework", run the Formatter project.
Also, check out the chanel 9 video mentioned above for a brief walk through.

Note
----
The project is built using VS2012 RC targeting the pre-release MVC 4 bits. The concept and code will work in VS2010 and MVC 3 but will require tweaking of the solution file, web.configs etc.