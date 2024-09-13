/*
 * Copyright (C) [2024] Shirlineyn (shirlineyn@gmail.com)
 *
 * Этот код распространяется под лицензией GNU General Public License версии 3.
 * Вы можете использовать, копировать и изменять его при соблюдении условий лицензии.
 */

using genetic_algorithm;

Route.map = new float[,]
{
    { 0, 6, 4, 8, 7, 14 },
    { 6, 0, 7, 11, 7, 10 },
    { 4, 7, 0, 4, 3, 10 },
    { 8, 11, 4, 0, 5, 11 },
    { 7, 7, 3, 5, 0, 7 },
   { 14, 10, 10, 11, 7, 0 }
};

Route.N = 6;

Population<Route> population = new Population<Route>(20, 4, 1);

bool isRunning = true;
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    isRunning = false;
};
int generation = 0;
while (isRunning)
{
    generation++;
    population.Mutate();
    population.Crossover();
    population.SelectByFight();

    Console.Clear();
    Console.WriteLine("Generation: " + generation);
    Console.WriteLine("Best 10 routes in generation (Press Ctrl+C to end algorithm):");
    Console.Write(population.bestToString(10));

    System.Threading.Thread.Sleep(1000);
}

Console.Clear();
Console.WriteLine("The best routes are:\n" + population.bestToString(5));





