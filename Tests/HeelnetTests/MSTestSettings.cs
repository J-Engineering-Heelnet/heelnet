// Selenium-testen delen één database en één draaiende applicatie. Parallel draaien zou
// testen elkaars testdata laten overschrijven, dus sequentieel.
[assembly: DoNotParallelize]
