# rainyroute-api-dotnet

just a project to play around and learn.

There is also a vuejs frontend: 

## What does it do?

it takes two sets of coordinates and sends it to [OSRM](https://project-osrm.org) to get 
a calculated route. Then we make another request to [weather api]((https://www.weatherapi.com/)) on the route at the 
specific time.

The plan is to then manipulate the route and startdate to get the most of a specific weather condition, like "I WANT IT TO RAIN ALL DAY" and rainyroute will help you.

## configuration
right now you just need to change the api key in appsettings.json or add a user-secret called "WeatherApiKey"
