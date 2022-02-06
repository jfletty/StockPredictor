# Stock Predictor
A collection of solutions that allowed me to semi successfully predict the stock market until the costs got too high, so the project was shutdown.

### Project Background
This project was started during the peak of the Covid 19 pandemic around the time when stock markets were crashing, and 'Meme' stocks were soaring.

I wanted to use my background of software engineering and predictive analytics to see if I could build a system that could initially give me predictions for any given stock, but then in the future, make those trades for me. In short, I was trying to build [a Quantitative Analysis Machine](https://en.wikipedia.org/wiki/Quantitative_analysis_(finance)).

#### Sourcing Data
At the beginning, I knew that behind all great algorithms was a massive dataset. The bigger and richer the better. I initially thought I would begin local and see what kind of API's the NZX (New Zealand Stock Exchange) could provide me. I reached out and had a meeting where they explained their product. It was loosely and API but in reality, it was a direct steaming feed of all market data provided in a very raw format. Although this was what I needed, after a month of attempting to get any useful data from it I walked away with only more questions. The price tag of over $10,000 NZD/month was a turnoff when the project was only a Proof of Concept (PoC) at this phase.

I continued looking around and found that Yahoo has market data for every stock exchange in the world and with a little bit of API sniffing, I could access these from a C# application. My theory was that I could build out my PoC using this semi-illegal data and if I proved I could get it working and make money from it, I could move to a legal, paid solution.

#### Choice of Algorithm
When choosing the type of algorithm for this project, I didn't give it too much thought (should have done research in hindsight) as I had previously used a `RandomForest` algorithm in the past for a similar solution. My theory was that I could build and continue to extend up my data model logic and the prediction would get smarter and smarter.

### Description of Solutions
This project contains 3 solutions all of which are required to get the project working.

**StockPredictor.Datawarehouse**

This C# project builds and can deploy an entire Datawarehouse to any SQL Server. I originally hosted this on AWS RDS which then became very expensive as you need to pay Microsoft for the SQL Server licence if you want any database over 10GB.
This database was very scalable and by the end of the project, had over 450GB of data with no signs of performance issues.

**StockPredictor.DataRetriever**

This C# solution is a collection of processors that retrieve all relevant market data from various sources, mainly being Yahoo.
* Symbol Retriever: This uses the FinnHub API to retrieve a list of all exchanges around the world and their related exchange key.
* Stock Retriever: Now that we have a list of exchanges, we can loop through these and retrieve all stocks/tickers underneath that exchange.
* Price Retriever: This service loops through all stocks/tickers and retrieves a detailed list of market changes for a given stock. This data is taken at a minutely internal and takes the last 2 days of history. This endpoint is limited to 7 days so was unable to get detailed data going back in time.
* Daily Price Retriever: Unlike the Price Retriever, this service collects data at a Daily level and much lower level data. This processor retrieves only the 7 days of market changes as the Historic Daily Retriever takes care of historic data.
* Historic Daily Price Retriever: This processor is only required once per stock/ticker and will retrieve all data related to that ticker as far back as the Yahoo can provide. For some tickers, this is over 10 years' worth.

**StockPredictor.Algorithm**

This C# solution is also a collection of processors that take data from the Datawarehouse, send it to S3 and trigger Python scripts to train the model and create the predictions.

* Preparation Worker: This service ensures that any ticker requiring either a new Model to be trained or a new projection, that data is retrieved from the Datawarehouse and placed into an AWS S3 bucket.
* Python Worker: This is a C# service that just calls Python scripts with specific command line arguments that allow the script to gather work from S3 and complete the required work. This solution is only built to run on a Linux machine/docker container but can be easily adapted to a windows machine.
* Cleanup Worker: This simple service downloads a processed projections from S3, maps it to the database class and saves it to the database.

### Future Improvements / Extensions

**Tests**

There is an obvious lack of tests present in this repository which caused many delays when attempting to understand and identify where issues were occurring from in the past. Unit tests should be able to provide much of the coverage and Component tests could cover the remainder.

**Algorithm**

Although the current algorithm works, there are many more variations that can be added which can be used in parallel to improve the confidence in the system.
The ones I investigated were the following:

*  Sentiment Analysis: I began looking into this so there are fragments of this floating around in the solution.
* Sniffing Algorithms
* Statistical Arbitrage
* Pairs Trading

I'm sure there are many more improvements that could be made / cleaned up in all the solutions so feel free to reach out and any questions or request changes. Or even better, do them yourself and create a PR for me :).