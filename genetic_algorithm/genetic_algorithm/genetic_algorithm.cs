/*
 * Copyright (C) [2024] Shirlineyn (shirlineyn@gmail.com)
 *
 * Этот код распространяется под лицензией GNU General Public License версии 3.
 * Вы можете использовать, копировать и изменять его при соблюдении условий лицензии.
 */
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace genetic_algorithm
{
    /// <summary>
    /// Абстрактный класс для реализации собственного объекта генетического алгоритма
    /// </summary>
    public abstract class Entity
    {
        /// <summary>
        /// Мутирует объект в другой.
        /// </summary>
        /// <param name="mutationindex">Сила мутации</param>
        /// <returns>Результат мутации</returns>
        public abstract Entity Mutate(int mutationindex);
        /// <summary>
        /// Скрещивает объект с другим
        /// </summary>
        /// <param name="entity">Другой скрещиваемый объект</param>
        /// <returns>Результат скрещивания</returns>
        public abstract Entity Crossover(Entity entity);
        /// <summary>
        /// Ищет выживаемость индивида
        /// </summary>
        /// <returns>Индекс выживаемости</returns>
        public abstract double Fit();
        /// <summary>
        /// Стандартный конструктор
        /// </summary>
        public Entity() { }
        /// <summary>
        /// Конструктор копирования
        /// </summary>
        /// <param name="entity">Оригинальный объект</param>
        public Entity(Entity entity) { }
        /// <summary>
        /// Конструктор полностью случайного объекта
        /// </summary>
        public abstract override string ToString();
    }
    
   /// <summary>
   /// Класс для реализации генетического алгоритма
   /// </summary>
   /// <typeparam name="T">Класс, реализующий Entity</typeparam>
    public class Population<T> where T : Entity, new()
    {
        private List<T> entities;
        private int crossover_multiplicator;
        private int mutationindex;
        private int basepopulation;

        /// <summary>
        /// Конструктор, реализующий популяцию из N особей.
        /// </summary>
        /// <param name="N">Количество особей</param>
        /// <param name="mutationindex">Сила мутации</param>
        /// <param name="crossover_multiplicator">Сила размножения</param>
        public Population(int N, int mutationindex = 1, int crossover_multiplicator = 1)
        {
            this.basepopulation = N;
            this.mutationindex = mutationindex;
            this.crossover_multiplicator = crossover_multiplicator;

            entities = new List<T>();
            for (int i = 0; i < N; i++)
            {
                T entity = (T)Activator.CreateInstance(typeof(T));
                entities.Add(entity);
            }
        }
        /// <summary>
        /// Проводит скрещивания между случайными объектами популяции в количестве
        /// (численность базовой популяции)*(сила размножения), добавляя результаты скрещивания в популяцию
        /// </summary>
        public void Crossover()
        {
            for (int i = 0; i < crossover_multiplicator * basepopulation; i++)
            {
                T father = entities[Random.Shared.Next(entities.Count)];
                T mother = entities[Random.Shared.Next(entities.Count)];
                T kid = (T)father.Crossover(mother);
                entities.Add(kid);
            }
        }
        /// <summary>
        /// [Параллельная версия]
        /// Проводит скрещивания между случайными объектами популяции в количестве
        /// (численность базовой популяции)*(сила размножения), добавляя результаты скрещивания в популяцию
        /// </summary>
        public void CrossoverP()
        {
            ConcurrentBag<T> kids = new ConcurrentBag<T>();
            Parallel.For(0, basepopulation * crossover_multiplicator, index =>
            {
                T father = entities[Random.Shared.Next(basepopulation)];
                T mother = entities[Random.Shared.Next(basepopulation)];
                kids.Add((T)father.Crossover(mother));
            });
            entities.AddRange(kids);
        }

        /// <summary>
        /// Проводит мутации для случайных объектов популяции в количестве силы мутации,
        /// добавляя результаты в популяцию
        /// </summary>
        public void Mutate()
        {
            for (int i = 0; i < basepopulation * mutationindex; i++)
            {
                 entities.Add((T)entities[Random.Shared.Next(entities.Count)].Mutate(mutationindex));
            }
        }
        /// <summary>
        /// [Параллельная версия]
        /// Проводит мутации для каждого объекта популяции в количестве силы мутации,
        /// добавляя результаты в популяцию
        /// </summary>
        public void MutateP()
        {
            ConcurrentBag<T> kids = new ConcurrentBag<T>();
            Parallel.For(0, basepopulation * mutationindex, index =>
            {
                kids.Add((T)entities[Random.Shared.Next(entities.Count)].Mutate(mutationindex));
            });
            entities.AddRange(kids);
        }
        /// <summary>
        /// Проводит селекцию в популяции путем сортировки по выживаемости, оставляя число особей
        /// в изначальной популяции
        /// </summary>
        public void SelectBySort()
        {
            sortEntities();
            if (entities.Count > basepopulation)
            {
                entities.RemoveRange(basepopulation, entities.Count - basepopulation);
            }
        }
        /// <summary>
        /// [Параллельная версия]
        /// Проводит селекцию в популяции путем сортировки по выживаемости, оставляя число особей
        /// в изначальной популяции
        /// </summary>
        public void SelectBySortP()
        {
            sortEntitiesP();
            if (entities.Count > basepopulation)
            {
                entities.RemoveRange(basepopulation, entities.Count - basepopulation);
            }
        }
        /// <summary>
        /// Проводит селецию в популяции путем голодных игр (попарные поединки, выживает сильнейший)
        /// до достижения изначальной популяции
        /// </summary>
        public void SelectByFight()
        {
            while (entities.Count > basepopulation)
            {
                int fighter1index = Random.Shared.Next(entities.Count);
                int fighter2index = Random.Shared.Next(entities.Count);
                if (fighter1index == fighter2index) continue;
                T fighter1 = entities[fighter1index];
                T fighter2 = entities[fighter2index];
                if (fighter1.Fit() >= fighter2.Fit())
                {
                    entities.RemoveAt(fighter2index);
                }
                else
                {
                    entities.RemoveAt(fighter1index);
                }
            }
        }
        /// <summary>
        /// Возвращает количество особей в популяции
        /// </summary>
        /// <returns>Количество особей</returns>
        public int Count() { return entities.Count; }
        /// <summary>
        /// Сортирует популяцию по выживаемости, начиная с максимальной
        /// </summary>
        public void sortEntities()
        {
            var fitMap = new Dictionary<Entity, double>();
            for (int i = 0; i < entities.Count; i++)
            {
                fitMap[entities[i]] = entities[i].Fit();
            }
            entities.Sort((e1, e2) => fitMap[e2].CompareTo(fitMap[e1]));
            //entities.Sort((e1, e2) => e2.Fit().CompareTo(e1.Fit()));
        }
        /// <summary>
        /// [Параллельная версия]
        /// Сортирует популяцию по выживаемости, начиная с максимальной
        /// </summary>
        public void sortEntitiesP() 
        {
            var fitMap = new ConcurrentDictionary<Entity, double> ();
            List<Task> tasks = new List<Task>();
            Parallel.For(0, entities.Count, index =>
            {
                int j = (int)index;
                fitMap[entities[j]] = entities[j].Fit();
            });

            entities.Sort((e1, e2) => fitMap[e2].CompareTo(fitMap[e1]));
        }
        
        /// <summary>
        /// Возвращает набор лучших по порядку особей
        /// </summary>
        /// <param name="n">Количество особей</param>
        /// <returns>Список особей</returns>
        public List<T> bestOfN(int n = 1)
        {
            List<T> result = new List<T>();
            sortEntities();
            for (int i = 0; i < n; i++)
            {
                result.Add(entities[i]);
            }
            return result;
        }
        /// <summary>
        /// [Параллельная версия]
        /// Возвращает набор лучших по порядку особей
        /// </summary>
        /// <param name="n">Количество особей</param>
        /// <returns>Список особей</returns>
        public List<T> bestOfNP(int n = 1)
        {
            List<T> result = new List<T>();
            sortEntitiesP();
            for (int i = 0; i < n; i++)
            {
                result.Add(entities[i]);
            }
            return result;
        }
        /// <summary>
        /// Возвращает набор лучших по порядку особей в виде строки
        /// </summary>
        /// <param name="n">Количество особей</param>
        /// <returns>Строка с особью</returns>
        public string bestToString(int n = 1)
        {
            sortEntities();
            string result = string.Empty;
            for (int i = 0; i < n; i++)
            {
                result += entities[i].ToString() + "\n"; 
            }
            return result;
        }
        /// <summary>
        /// [Параллельная версия]
        /// Возвращает набор лучших по порядку особей в виде строки
        /// </summary>
        /// <param name="n">Количество особей</param>
        /// <returns>Строка с особью</returns>
        public string bestToStringP(int n = 1)
        {
            sortEntitiesP();
            string result = string.Empty;
            for (int i = 0; i < n; i++)
            {
                result += entities[i].ToString() + "\n";
            }
            return result;
        }
        /// <summary>
        /// Возвращает всю популяцию в виде строки
        /// </summary>
        /// <returns>Строка с популяцией</returns>
        public override string ToString()
        {
            string result = string.Empty;
            foreach (var entity in entities)
            {
                result += entity.ToString() + '\n';
            }
            return result;
        }
    }

    public class Route : Entity
    {
        public static float[,] map { get; set; }
        public static int N { get; set; }
        public List<int> route;
        public Route(Route oldroute)
        {
            this.route = new List<int>(oldroute.route);
        }
        public Route(List<int> route)
        {
            this.route = new List<int>(route);
        }
        public Route()
        {
            route = Enumerable.Range(1, N).ToList();
            for (int i = N - 1; i > 0; i--)
                swapelems(i, Random.Shared.Next(i + 1));
        }
        private void swapelems(int elem1, int elem2)
        {
            int tmp = route[elem1];
            route[elem1] = route[elem2];
            route[elem2] = tmp;
        }

        public override Route Mutate(int mutationindex)
        {
            Route newroute = new Route(this);
            for (int i = 0; i < Random.Shared.Next(mutationindex); i++)
            {
                newroute.swapelems(Random.Shared.Next(N), Random.Shared.Next(N));
            }
            return newroute;
        }

        public override Route Crossover(Entity entity)
        {
            int start = Random.Shared.Next(N);
            int end = Random.Shared.Next(N);
            if (start > end)
            {
                (start, end) = (end, start);
            }
            Route resultRoute = new Route(this);

            int currentIndex = (end + 1) % N;
            for (int i = 0; i < N - (end - start); i++)
            {
                int parent2Index = (end + 1 + i) % N;
                int value = ((Route)entity).route[parent2Index];
                if (resultRoute.route.IndexOf(value) == -1)
                {
                    resultRoute.route[currentIndex] = value;
                    currentIndex = (currentIndex + 1) % N;
                }
            }
            return resultRoute;
        }
        public override double Fit()
        {
            double result = 0.0;
            for (int i = 0; i < N - 1; i++)
            {
                result += map[this.route[i] - 1, this.route[i + 1] - 1];
            }
            result += map[this.route[N - 1] - 1, this.route[0] - 1];
            return -result;
        }
        public override string ToString()
        {
            return string.Join("-", route) + "; L=" + -Fit();
        }
    }
}
