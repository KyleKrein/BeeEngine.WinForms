using BeeEngine.Vector;

namespace BeeEngine
{
    //Пример генератора шума Перлина
    public sealed class PerlinNoiseGenerator
    {
        private byte[] permutationTable;

        public PerlinNoiseGenerator(int seed = 0)
        {
            var rand = new Random(seed);
            permutationTable = new byte[1024];
            rand.NextBytes(permutationTable); // заполняем случайными байтами
        }

        private Vector2 GetPseudoRandomGradientVector(int x, int y)
        {
            // хэш-функция с Простыми числами, обрезкой результата до размера массива со случайными байтами (операция & приводит число к значению от 0 до 3 и работает быстрее чем %)// алгоритм был частично взят с https://habr.com/ru/post/430384/ (в частности этот метод), так как другие способы показывали результаты хуже
            //& - логическое и
            //^ - логическое или (XOR)
            int v = (int)(((x * 1836311903) ^ (y * 2971215073) + 4807526976) & 1023);
            v = permutationTable[v] & 3;
            /*
            у каждой точки 2 координаты.
            Для начала всем точкам плоскости дается случайный вектор. 
            В оригинальном алгоритме это был любой вектор единичной длины, в улучшенной версии алгоритма 
            должен быть 1 из следующих векторов: (1, 0), (-1, 0), (0, 1), (0, -1).
            ниже указаны как раз хранит эти вектора
            */
            switch (v)
            {
                case 0: return new Vector2(1, 0);
                case 1: return new Vector2(-1, 0);
                case 2: return new Vector2(0, 1);
                default: return new Vector2(0, -1);
            }
        }

        //Эта функция как бы прижимает значение к одному из краев отрезка [0, 1]. Т.е. если X близок к 0, то Fade(X) еще ближе к 0
        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float GetWeightedAverage(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        //функция скалярного произведения двух векторов 
        private static float MultiplyVectors(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        /*
        При генерации значения для точки (x, y) находим ближайшие к ней точки с целочисленными координатами (x0, y0), (x0, y1), (x1, y0), (x1, y1).
        Далее для каждой точки считаем скалярное произведение двух векторов: случайного, сгенерированного ранее, и вектора до точки (x, y).
        */
        public float Noise(float fx, float fy)
        {
            fx = fx / 10f;
            fy = fy / 10f;
            int left = (int)fx.Floor();
            int top = (int)fy.Floor();
            // а теперь локальные координаты точки внутри квадрата
            float pointInQuadX = fx - left;
            float pointInQuadY = fy - top;

            // извлекаем градиентные (показывает направление возрастания скалярной величины и равен скорости ее возрастания в этом
            // направлении) векторы для всех вершин квадрата:
            Vector2 topLeftGradient = GetPseudoRandomGradientVector(left, top);
            Vector2 topRightGradient = GetPseudoRandomGradientVector(left + 1, top);
            Vector2 bottomLeftGradient = GetPseudoRandomGradientVector(left, top + 1);
            Vector2 bottomRightGradient = GetPseudoRandomGradientVector(left + 1, top + 1);

            // вектора от вершин квадрата до точки внутри квадрата:
            Vector2 distanceToTopLeft = new Vector2 ( pointInQuadX, pointInQuadY );
            Vector2 distanceToTopRight = new Vector2 ( pointInQuadX - 1, pointInQuadY );
            Vector2 distanceToBottomLeft = new Vector2 ( pointInQuadX, pointInQuadY - 1 );
            Vector2 distanceToBottomRight = new Vector2 ( pointInQuadX - 1, pointInQuadY - 1 );

            // считаем скалярные произведения
            /*
             tx1--tx2
              |    |
             bx1--bx2
            */
            float tx1 = MultiplyVectors(distanceToTopLeft, topLeftGradient);
            float tx2 = MultiplyVectors(distanceToTopRight, topRightGradient);
            float bx1 = MultiplyVectors(distanceToBottomLeft, bottomLeftGradient);
            float bx2 = MultiplyVectors(distanceToBottomRight, bottomRightGradient);


            /*
            Мы получили случайные числа для каждой из соседних целочисленных точек. 
            Теперь считаем средневзвешенное значение. 
            В качестве веса используем расстояние до точки (x, y) по каждой из осей. 
            */
            pointInQuadX = Fade(pointInQuadX);
            pointInQuadY = Fade(pointInQuadY);

            float tx = GetWeightedAverage(tx1, tx2, pointInQuadX);
            float bx = GetWeightedAverage(bx1, bx2, pointInQuadX);
            float tb = GetWeightedAverage(tx, bx, pointInQuadY);
            return tb;
        }
    }
}
