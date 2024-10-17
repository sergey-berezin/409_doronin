/*
 * Copyright (C) [2024] Shirlineyn (shirlineyn@gmail.com)
 *
 * Этот код распространяется под лицензией GNU General Public License версии 3.
 * Вы можете использовать, копировать и изменять его при соблюдении условий лицензии.
 */
using System.Diagnostics;

using genetic_algorithm;

//Route.map = new float[,]
//{
//    { 0, 6, 4, 8, 7, 14 },
//    { 6, 0, 7, 11, 7, 10 },
//    { 4, 7, 0, 4, 3, 10 },
//    { 8, 11, 4, 0, 5, 11 },
//    { 7, 7, 3, 5, 0, 7 },
//   { 14, 10, 10, 11, 7, 0 }
//};
Route.map = new float[,]
{
    { 0f, 42f, 91f, 13f, 67f, 85f, 32f, 49f, 75f, 28f, 19f, 76f, 58f, 24f, 41f, 93f, 71f, 35f, 22f, 98f },
    { 42f, 0f, 76f, 31f, 59f, 88f, 15f, 23f, 10f, 70f, 5f, 18f, 77f, 21f, 16f, 30f, 55f, 47f, 84f, 27f },
    { 91f, 76f, 0f, 79f, 25f, 34f, 43f, 69f, 35f, 48f, 92f, 82f, 60f, 89f, 52f, 30f, 65f, 80f, 54f, 33f },
    { 13f, 31f, 79f, 0f, 82f, 60f, 89f, 17f, 66f, 38f, 92f, 37f, 83f, 11f, 62f, 48f, 95f, 40f, 81f, 61f },
    { 67f, 59f, 25f, 82f, 0f, 51f, 52f, 90f, 49f, 38f, 36f, 91f, 51f, 14f, 77f, 55f, 72f, 40f, 45f, 84f },
    { 85f, 88f, 34f, 60f, 51f, 0f, 43f, 69f, 66f, 22f, 48f, 60f, 33f, 77f, 16f, 21f, 65f, 54f, 87f, 63f },
    { 32f, 15f, 43f, 89f, 52f, 43f, 0f, 17f, 76f, 29f, 92f, 38f, 68f, 11f, 62f, 48f, 95f, 40f, 81f, 74f },
    { 49f, 23f, 69f, 17f, 90f, 69f, 17f, 0f, 37f, 47f, 96f, 25f, 82f, 12f, 55f, 72f, 40f, 61f, 45f, 27f },
    { 75f, 10f, 35f, 66f, 49f, 66f, 76f, 37f, 0f, 53f, 66f, 38f, 70f, 14f, 19f, 33f, 80f, 54f, 84f, 64f },
    { 28f, 70f, 48f, 38f, 38f, 22f, 29f, 47f, 53f, 0f, 66f, 22f, 38f, 16f, 19f, 63f, 32f, 75f, 31f, 64f },
    { 19f, 5f, 92f, 82f, 36f, 48f, 92f, 96f, 66f, 66f, 0f, 38f, 70f, 14f, 63f, 15f, 57f, 27f, 87f, 19f },
    { 76f, 18f, 82f, 37f, 91f, 60f, 38f, 25f, 38f, 22f, 38f, 0f, 91f, 16f, 77f, 55f, 72f, 40f, 45f, 67f },
    { 58f, 77f, 89f, 83f, 14f, 33f, 11f, 12f, 70f, 16f, 70f, 91f, 0f, 51f, 77f, 62f, 48f, 95f, 39f, 31f },
    { 24f, 16f, 52f, 11f, 77f, 16f, 62f, 55f, 19f, 19f, 63f, 77f, 51f, 0f, 77f, 62f, 48f, 95f, 39f, 31f },
    { 41f, 93f, 93f, 62f, 16f, 21f, 48f, 72f, 33f, 63f, 15f, 77f, 77f, 77f, 0f, 77f, 48f, 95f, 39f, 31f },
    { 71f, 98f, 71f, 48f, 74f, 63f, 81f, 20f, 75f, 46f, 57f, 40f, 62f, 31f, 77f, 0f, 77f, 48f, 95f, 39f },
    { 35f, 22f, 80f, 95f, 48f, 54f, 95f, 40f, 54f, 75f, 57f, 40f, 48f, 31f, 77f, 77f, 0f, 77f, 39f, 78f },
    { 22f, 98f, 54f, 81f, 40f, 87f, 81f, 61f, 84f, 31f, 27f, 67f, 48f, 31f, 77f, 48f, 77f, 0f, 78f, 53f },
    { 98f, 76f, 33f, 39f, 45f, 63f, 57f, 27f, 64f, 31f, 19f, 67f, 31f, 31f, 31f, 39f, 78f, 53f, 0f, 78f },
    { 46f, 70f, 22f, 29f, 88f, 63f, 20f, 57f, 64f, 64f, 19f, 40f, 31f, 31f, 31f, 39f, 78f, 53f, 78f, 0f }
};

Route.N = 20;

Population<Route> population = new Population<Route>(1000, 1000, 1000);
Population<Route> ppopulation = new Population<Route>(1000, 100, 100);

bool isRunning = true;
Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    isRunning = false;
};

Stopwatch stopwatch = new Stopwatch();
int generation = 0;
while (isRunning)
{
    generation++;



    //stopwatch.Start();
    //population.Mutate();
    //stopwatch.Stop();

    //Console.Clear();
    //Console.WriteLine("Generation: " + generation);
    //Console.WriteLine($"Regular mutation: {stopwatch.ElapsedMilliseconds}ms");


    //stopwatch.Restart();
    //population.Mutate();
    //stopwatch.Stop();
    //Console.WriteLine($"Regular mutation: {stopwatch.ElapsedMilliseconds}ms");

    //System.Threading.Thread.Sleep(200);

    //stopwatch.Restart();
    //ppopulation.MutateP();
    //stopwatch.Stop();
    //Console.WriteLine($"Parallel mutation: {stopwatch.ElapsedMilliseconds}ms");


    //stopwatch.Restart();
    //population.Crossover();
    //stopwatch.Stop();
    //Console.WriteLine($"Regular crossover: {stopwatch.ElapsedMilliseconds}ms");
    //System.Threading.Thread.Sleep(200);
    //stopwatch.Restart();
    //ppopulation.CrossoverP();
    //stopwatch.Stop();
    //Console.WriteLine($"Parallel crossover(pfor): {stopwatch.ElapsedMilliseconds}ms");
    //stopwatch.Restart();
    //population.CrossoverP();
    //stopwatch.Stop();
    //Console.WriteLine($"Parallel crossover: {stopwatch.ElapsedMilliseconds}ms");



    //stopwatch.Restart();
    //var taskMutate = Task.Run(() => ppopulation.MutateP());
    //var taskCrossover = Task.Run(() => ppopulation.CrossoverP());
    //await taskMutate;
    //await taskCrossover;
    //stopwatch.Stop();
    //Console.WriteLine($"Parallel crossover+mutate: {stopwatch.ElapsedMilliseconds}ms");

    //stopwatch.Restart();
    //population.SelectBySort();
    //stopwatch.Stop();
    //Console.WriteLine($"Regular select by sort: {stopwatch.ElapsedMilliseconds}ms");

    //System.Threading.Thread.Sleep(200);

    //stopwatch.Restart();
    //ppopulation.SelectBySortP();
    //stopwatch.Stop();
    //Console.WriteLine($"Parallel select by sort: {stopwatch.ElapsedMilliseconds}ms");

    //stopwatch.Restart();
    //ppopulation.SelectBySortP();
    //stopwatch.Stop();
    //Console.WriteLine($"Parallel select by sort: {stopwatch.ElapsedMilliseconds}ms");

    population.MutateP();
    population.CrossoverP();
    population.SelectBySortP();


    //Console.WriteLine("Best 10 routes in generation (Press Ctrl+C to end algorithm):");
    //Console.Write(population.bestToStringP(10));

    //System.Threading.Thread.Sleep(500);
}

//Console.Clear();
Console.WriteLine("The best routes are:\n" + population.bestToString(5));





