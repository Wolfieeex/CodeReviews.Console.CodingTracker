# Coding Tracker
## Brief Documentation
### What is coding tracker?
The Coding Tracker is the ultimate productivity companion for devs who want some insight into their workflow. Built on a simple Create–Read–Update–Delete (CRUD) model, it gives you total control over every programming session you log. Start the built‑in timer when you dive into your code, or add sessions manually if you prefer. Then, whenever you need to amend an entry, for example, updating the number of lines you wrote, you can edit it in seconds. When it’s time to reflect, the Tracker’s reports lay out your past work in an easy-to-read format, helping you identify patterns, spot weaknesses, and plan your future steps. And if an old record no longer serves you, just remove it. This is your database, and so your rules.

On top of session logging, Coding Tracker helps you stay focused on your goals. Set milestones for your projects, and watch as each new coding session automatically brings you closer to, or warns you of, upcoming deadlines. When you’re approaching completion, you’ll get a heads‑up. If circumstances change, simply remove any goal you no longer need. No matter how your projects evolve, Coding Tracker keeps your planning as dynamic as your development.

### Features
1) #### Session injection- adding coding session manually 
   * Use this menu to record a session you didn’t track with the built‑in timer. The start and end dates are required. Optional but recommended: number of lines and comments—these make future reviews clearer.
   * Each field is validated to enforce the correct format, so the database stays clean and consistent. You can update or clear any field by using special inputs, and the program blocks entries whose start date is after the end date.
   * Once saved, the session is written to the database. Important! Manually added sessions do not update goals.
   * 
    _Menu screenshot_
2) #### Session timer tracking
   * Start the timer when you begin coding; a large timer appears in the centre of the console and counts seconds. You can pause it at any time.
   * You can discard a running session if you decide not to keep it.
   * When you finish, enter the number of lines and comments. A new record is then added to the database.
3) #### Goal features
   * Add goals to track either lines of code produced or time spent coding within a chosen timeframe. Set a deadline from presents, or create a custom one, as you wish.
   * View previous goals, whether they are failed, completed, or in progres in a data table that shows all key details. You can delete goal history you no longer need.
   * When a goal is no longer relevant, simply delete it from the Goals menu.
4) #### Filtering
   * When creating reports, deleting, updating, or viewing data, filter out records you don’t need by comments, lines, start/end dates, and more, so you never wander through unnecessary entries.
   * Sort by any column in ascending or descending order.
5) #### Delete menu
   * Remove session records you no longer want. First filter the records, then select the index (or comma‑separated indexes). You’ll be asked to confirm before deletion, as the action is irreversible.
6) #### Update menu
   * Entered something incorrectly? Filter to locate the record, choose its index, and amend any field.
7) #### Report menu
   * Analyse your coding activity at a deeper level: create reports that calculate sums, medians, averages, and more. Group data by year, month, week, or day, then apply additional filters as needed.
   * After the report is displayed, you can save it as a .csv format for archiving or further analysis.
8) #### View menu
   * Similar to the Report menu, but shows raw records only— no aggregation or transformation.
### Things I learned during this project
* Dapper ORM
  * Using queries using both anonymous types and objects
  * Adding paramaters to queries and assigning their values
* Sqlite
  * Injection of multiple conditions into SQL query to create Filtering Class
  * Using Sqlite functions, such as _INSTR_ or _SUBSTR_ to manipulate query data (such as dates and time span) for compability reasons
  * Utilising subqueries for specific reader requests
  * Using intermediate database commands, such as _HAVING, OFFSET + LIMIT_ to find median and modal values
* Spectre Console
  * Drawing user multi and single selection UI to switch between a variety of menus
  * Utilising datatables to show results of the reports and present selected data when changing or deleting it
  * 
* C# Documentation
  * 
