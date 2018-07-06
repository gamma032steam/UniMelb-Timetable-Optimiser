# University of Melbourne Timetable Optimiser
A WPF application that generates optimised subject timetables, given user-defined restrictions and preferences.

## Time Restrictions
One of the most obvious and useful things you can do is try restrict your day spans. Users can specify **both** an 'Earliest Start' and 'Latest Finish' time.
## General Optimisations
A variety of useful optimisations options are provided, namely:

 - Least Clashes
	 - Aims to reduce the number of clashes present in the timetable. Expectedly the most useful and desired optimisations a user may want.
 - Cram
	 - Seeks to stuff as many *variable* classes into the least number of days possible.
 - Day Avoidance
	 - Don't like Mondays? That's fine. Job on Wednesday aswell? Handled.
 - Longest Run Without a Break
	 - Aims to reduce the number of consecutive classes for a user defined time span without a break. (e.g. no more than 3 hours without a break)
## Optimisation Order
The general optimisations often have different priorities. These can be intuitively reordered before optimisation in terms of priority. 

![Main UI Example](https://i.imgur.com/39VSW2S.png)
