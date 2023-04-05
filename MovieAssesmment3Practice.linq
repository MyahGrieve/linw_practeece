<Query Kind="Program">
  <Connection>
    <ID>2054d066-339e-449b-b5f4-aa4025a796d8</ID>
    <NamingServiceVersion>2</NamingServiceVersion>
    <Persist>true</Persist>
    <Driver Assembly="(internal)" PublicKeyToken="no-strong-name">LINQPad.Drivers.EFCore.DynamicDriver</Driver>
    <Server>DESKTOP-A3C7HDD</Server>
    <Database>GroceryList</Database>
    <DisplayName>GroceryListEntity</DisplayName>
    <DriverData>
      <EncryptSqlTraffic>True</EncryptSqlTraffic>
      <PreserveNumeric1>True</PreserveNumeric1>
      <EFProvider>Microsoft.EntityFrameworkCore.SqlServer</EFProvider>
      <EFVersion>6.0.12</EFVersion>
      <TrustServerCertificate>True</TrustServerCertificate>
    </DriverData>
  </Connection>
</Query>

void Main()
{
	/*
	   To DO:
	   -what if the datetime coming in from the movieBookingRequest is not on the correct date?
	*/
}

public void ScheduleMovie(TheatreBookingView theatreBookingRequest)
{
	List<Exception> errorlist = new List<Exception>();
	TheatreBooking theatreBooking = new TheatreBooking();

	if (theatreBookingRequest == null)
	{
		throw new ArgumentNullException("The booking request is empty. Please fill in all the fields");
	}
	//all scheduling slots are filled
	if (theatreBookingRequest.MovieBookings.Count() < 4)
	{
		errorlist.Add(new Exception("Please ensure all 4 time slots are filled"));
	}

	//gets 11am
	DateTime bookingDateAM = new DateTime(2023, 4, 4, 11, 0, 0);
	TimeSpan elevenAM = bookingDateAM.TimeOfDay;

	//gets 11pm
	DateTime bookingDatePM = new DateTime(2023, 4, 4, 23, 0, 0);
	TimeSpan elevenPM = bookingDatePM.TimeOfDay;

	//gets 20 minutes
	int gapLengthInMinutes = 20;
	TimeSpan gapLength = TimeSpan.FromMinutes(gapLengthInMinutes);
	
	//check to see if any movies start before 11am or end after 11pm
	foreach (var movieBookingRequest in theatreBookingRequest.MovieBookings)
	{
		//not before 11am and not after 11pm
		if (movieBookingRequest.StartTime.TimeOfDay < elevenAM)
		{
			errorlist.Add(new Exception($"Movie: {movieBookingRequest.Title} scheduled at {movieBookingRequest.StartTime.TimeOfDay} cannot start before 11am."));
		}

		//if the movie.starttime + the lnegth is after 11pm
		var movieRequestLength = Movies.Where(x => x.MovieID == movieBookingRequest.MovieID).Select(x => x.Length).FirstOrDefault();

		TimeSpan movieLength = TimeSpan.FromMinutes(movieRequestLength);
		TimeSpan movieEndTime = movieBookingRequest.StartTime.TimeOfDay + movieLength + gapLength;

		if (movieEndTime > elevenPM)
		{
			errorlist.Add(new Exception($"Movie: {movieBookingRequest.Title} scheduled at {movieBookingRequest.StartTime.TimeOfDay} cannot end after 11pm."));

		}
	}

	//no overlap and minimum 20 minute spaces
	for (int i = 0; i < 4; i++)
	{
		MovieBookingView firstMovie = theatreBookingRequest.MovieBookings[i];
		MovieBookingView secondMovie = theatreBookingRequest.MovieBookings[i + 1];
		
		var movieRequestLength = Movies.Where(x => x.MovieID == firstMovie.MovieID).Select(x => x.Length).FirstOrDefault();

		TimeSpan movieLength = TimeSpan.FromMinutes(movieRequestLength); // length in datetime of first movie
		TimeSpan firstMovieEndTime = firstMovie.StartTime.TimeOfDay + movieLength; //endtime of first movie


		if (firstMovieEndTime > secondMovie.StartTime.TimeOfDay) // if the endtime of the first movie is greater than the starttime of the secondmovie
		{
			errorlist.Add(new Exception($"Movie: {firstMovie.Title} at {firstMovie.StartTime.TimeOfDay} overlaps with {secondMovie.Title} at {secondMovie.StartTime.TimeOfDay} Please adjust movie times"));
		}
		if ((firstMovieEndTime + gapLength) >= secondMovie.StartTime.TimeOfDay)
		{
			errorlist.Add(new Exception($"Movie: {secondMovie.Title} cannot start before previous movie ends and is cleaned up. Please adjust movie times"));
		}

	}
	
	foreach (var movieBookingRequest in theatreBookingRequest.MovieBookings)
	{
		ShowTimes newShowTime = new ShowTimes();
		newShowTime.MovieID = movieBookingRequest.MovieID;
		newShowTime.StartDate = movieBookingRequest.StartTime;
		newShowTime.TheatreID = theatreBookingRequest.TheatreID;
		ShowTimes.Add(newShowTime);
	}
	
	
	
	if (errorlist.Count() == 0)
	{
		
		SaveChanges();
	}
	else
	{
		ChangeTracker.Clear();
		throw new AggregateException(errorlist);
	}
}

public class TheatreBookingView
{
	public int TheatreID { get; set; }
	public List<MovieBookingView> MovieBookings { get; set; }

}

public class MovieBookingView
{
	public int MovieID { get; set; }
	public DateTime StartTime { get; set; }
}
