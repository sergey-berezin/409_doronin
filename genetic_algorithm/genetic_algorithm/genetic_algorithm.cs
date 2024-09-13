/*
 * Copyright (C) [2024] Shirlineyn (shirlineyn@gmail.com)
 *
 * Этот код распространяется под лицензией GNU General Public License версии 3.
 * Вы можете использовать, копировать и изменять его при соблюдении условий лицензии.
 */
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;

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
        /// <param name="rnd">Рандомизатор</param>
        /// <param name="mutationindex">Сила мутации</param>
        /// <returns>Результат мутации</returns>
        public abstract Entity Mutate(Random rnd, int mutationindex);
        /// <summary>
        /// Скрещивает объект с другим
        /// </summary>
        /// <param name="entity">Другой скрещиваемый объект</param>
        /// <param name="rnd">Рандомизатор</param>
        /// <returns>Результат скрещивания</returns>
        public abstract Entity Crossover(Entity entity, Random rnd);
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
        /// <param name="random">Рандомизатор</param>
        public Entity(Random random) { }
        /// <summary>
        /// Возвращает представление объекта в виде строки
        /// </summary>
        /// <returns>Представление объекта в виде строки</returns>
        public abstract override string ToString();
    }
    
   /// <summary>
   /// Класс для реализации генетического алгоритма
   /// </summary>
   /// <typeparam name="T">Класс, реализующий Entity</typeparam>
    public class Population<T> where T : Entity, new()
    {
        private List<T> entities;
        private Random rnd;
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

            rnd = new Random();
            entities = new List<T>();
            for (int i = 0; i < N; i++)
            {
                T entity = (T)Activator.CreateInstance(typeof(T), rnd);
                entities.Add(entity);
            }
        }
        /// <summary>
        /// Проводит скрещивания между случайными объектами популяции в количестве
        /// (численность популяции)*(сила размножения), добавляя результаты скрещивания в популяцию
        /// </summary>
        public void Crossover()
        {
            int N = entities.Count;
            for (int i = 0; i < crossover_multiplicator * N; i++)
            {
                T father = entities[rnd.Next(N)];
                T mother = entities[rnd.Next(N)];
                T kid = (T)father.Crossover(mother, rnd);
                entities.Add(kid);
            }
        }
        /// <summary>
        /// Проводит мутации для каждого объекта популяции в количестве силы мутации,
        /// добавляя результаты в популяцию
        /// </summary>
        public void Mutate()
        {
            int N = entities.Count;
            for (int i = 0; i < N; i++)
            {
                for (int j = 0; j < mutationindex; j++)
                {
                    entities.Add((T)entities[i].Mutate(rnd, mutationindex));
                } 
            }
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
        /// Проводит селецию в популяции путем голодных игр (попарные поединки, выживает сильнейший)
        /// до достижения изначальной популяции
        /// </summary>
        public void SelectByFight()
        {
            while (entities.Count > basepopulation)
            {
                int fighter1index = rnd.Next(entities.Count);
                int fighter2index = rnd.Next(entities.Count);
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
        public void sortEntities() { entities.Sort((e1, e2) => e2.Fit().CompareTo(e1.Fit()));}
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
        private List<int> route;
        public Route()
        {
            route = Enumerable.Range(1, N).ToList();
        }
        public Route(Route oldroute)
        {
            this.route = new List<int>(oldroute.route);
        }
        public Route(List<int> route)
        {
            this.route = new List<int>(route);
        }
        public Route(Random rnd) : this()
        {
            
            for (int i = N - 1; i > 0; i--)
                swapelems(i, rnd.Next(i + 1));
        }
        private void swapelems(int elem1, int elem2)
        {
            int tmp = route[elem1];
            route[elem1] = route[elem2];
            route[elem2] = tmp;
        }

        public override Route Mutate(Random rnd, int mutationindex)
        {
            Route newroute = new Route(this);
            for (int i = 0; i < mutationindex; i++)
            {
                newroute.swapelems(rnd.Next(N), rnd.Next(N));
            }
            return newroute;
        }

        public override Route Crossover(Entity entity, Random rnd)
        {
            Route otherRoute = (Route)entity;
            Random rand = new Random();
            int start = rand.Next(N);
            int end = rand.Next(N);
            if (start > end)
            {
                (start, end) = (end, start);
            }
            List<int> resultroute = route;
            
            for (int i = start; i <= end; i++)
            {
                resultroute[i] = this.route[i];
            }
            int currentIndex = (end + 1) % N;
            for (int i = 0; i < N; i++)
            {
                int parent2Index = (end + 1 + i) % N;
                int value = otherRoute.route[parent2Index];
                if (resultroute.IndexOf(value) == -1)
                {
                    resultroute[currentIndex] = value;
                    currentIndex = (currentIndex + 1) % N;
                }
            }

            return new Route(resultroute);
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
