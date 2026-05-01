# News Scraper



## Description

This project is an ASP.NET & React Web Application which scrapes 5 different news websites and uses embedded based similarity comparison & keyword based
classification to classify news and uses a geocoding api to determine the locations of the events and places them on google maps in an interactive website.


## Features

- **Interactive Map**: Displays the location of the events in the news.
- **Embedded Based Similarity Search**: This project uses the nomic-embed-text model to embed the news article to the vector space, then a cosinus similarity
  calculation is executed and similar news articles get eliminated to combat duplicate news.
- **Keyword-Based Classification**: News articles go through a keyword based classification and get assigned to different news classes.
- **No-Sql Database**: This project uses Mongo-DB to store scraped news articles.
- **Article Cleanup**: The scraped websites go through multiple layered article cleanup process to remove ads, header and footer elements,
  unnecessary elements (such as comments section), html tags and elements and many more.
- **News Class Filters**: User is able to filter news based on the types of events.
- **News Location Filter**: User is able to filter news based on the location.
- **News Date Filter**: User is able to filter news based on the date of occurence.
- **Re-Scraping News**: User can re-scrape the news while still using the website. This happens in the background and a progress bar shows how much is left
  until the end of the task.
- **Links To The Article**: User can read the news article by pressing the link button.



## License

This project is licensed under the **Creative Commons Attribution-NonCommercial 4.0 International (CC BY-NC 4.0)** license.


