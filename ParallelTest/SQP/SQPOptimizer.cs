namespace OwnSQP
{ 
    public class SQPOptimizer
    {
        // Делегаты для целевой функции и ограничений
        public delegate double ObjectiveFunc(double[] x);
        public delegate double[] ConstraintsFunc(double[] x); // Возвращает массив g_i(x) <= 0
    
        /// <summary>
        /// Запуск оптимизации методом SQP
        /// </summary>
        public static double[] Minimize(
            ObjectiveFunc f,
            ConstraintsFunc g,
            double[] x0,
            double tol = 1e-6,
            int maxIter = 100)
        {
            int n = x0.Length;
            double[] x = (double[])x0.Clone();
    
            // Тестовый вызов для определения количества ограничений m
            int m = g(x).Length;
    
            // Инициализация матрицы Гессиана (единичная матрица I)
            double[,] h = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                h[i, i] = 1.0;
            }
    
            // Множители Лагранжа для ограничений
    
            for (int iter = 0; iter < maxIter; iter++)
            {
                // 1. Вычисляем значения и градиенты в текущей точке x
                f(x);
                double[] gVals = g(x);
    
                double[] gradF = ComputeGradient(f, x);
                double[,] jacG = ComputeJacobian(g, x, m, n);
    
                // 2. Решаем подзадачу QP (Quadratic Programming) для поиска направления d
                // Находим направление d и новые множители lambda_new
                double[] d = SolveQp(h, gradF, jacG, gVals, out double[] lambdaNew);
    
                // Проверка критерия останова (если шаг направления слишком мал)
                if (Norm(d) < tol)
                {
                    break;
                }
    
                // 3. Линейный поиск (Line Search) с функцией штрафа
                double alpha = LineSearch(f, g, x, d, gradF, gVals);
    
                // Вычисляем следующий шаг
                double[] xNext = new double[n];
                for (int i = 0; i < n; i++)
                {
                    xNext[i] = x[i] + (alpha * d[i]);
                }
    
                // 4. Обновление матрицы Гессиана по методу BFGS
                double[] gradFNext = ComputeGradient(f, xNext);
                g(xNext);
                double[,] jacGNext = ComputeJacobian(g, xNext, m, n);
    
                UpdateHessianBfgs(h, x, xNext, gradF, gradFNext, jacG, jacGNext, lambdaNew, m, n);
    
                // Переход к следующей итерации
                x = xNext;
            }
    
            return x;
        }
    
        /// <summary>
        /// Численное взятие градиента целевой функции (Центральная разность)
        /// </summary>
        private static double[] ComputeGradient(ObjectiveFunc f, double[] x, double h = 1e-6)
        {
            int n = x.Length;
            double[] grad = new double[n];
            double[] xTmp = (double[])x.Clone();
    
            for (int i = 0; i < n; i++)
            {
                xTmp[i] = x[i] + h;
                double f1 = f(xTmp);
                xTmp[i] = x[i] - h;
                double f2 = f(xTmp);
                grad[i] = (f1 - f2) / (2 * h);
                xTmp[i] = x[i];
            }
    
            return grad;
        }
    
        /// <summary>
        /// Численное вычисление матрицы Якоби для ограничений
        /// </summary>
        private static double[,] ComputeJacobian(ConstraintsFunc g, double[] x, int m, int n, double h = 1e-6)
        {
            double[,] jac = new double[m, n];
            double[] xTmp = (double[])x.Clone();
    
            for (int j = 0; j < n; j++)
            {
                xTmp[j] = x[j] + h;
                double[] g1 = g(xTmp);
                xTmp[j] = x[j] - h;
                double[] g2 = g(xTmp);
    
                for (int i = 0; i < m; i++)
                {
                    jac[i, j] = (g1[i] - g2[i]) / (2 * h);
                }
    
                xTmp[j] = x[j];
            }
    
            return jac;
        }
    
        /// <summary>
        /// Простейший QP-решатель методом активного набора (Active Set) для подзадач SQP
        /// </summary>
        private static double[] SolveQp(double[,] h, double[] gradF, double[,] jacG, double[] gVals, out double[] lambda)
        {
            int n = gradF.Length;
            int m = gVals.Length;
            double[] d = new double[n];
            lambda = new double[m];
    
            // Для демонстрации базового алгоритма решаем ККТ-систему через псевдообратные матрицы
            // В промышленном коде здесь используется метод проецируемых градиентов или Гольдфарба-Иднани.
            // Здесь мы находим активные ограничения (те, которые нарушены или близки к 0)
            List<int> activeSet = new();
            for (int i = 0; i < m; i++)
            {
                if (gVals[i] >= -1e-4)
                {
                    activeSet.Add(i);
                }
            }
    
            int k = activeSet.Count;
            if (k == 0)
            {
                // Без ограничений: d = -H^-1 * grad_f
                d = SolveLinearSystem(h, Multiply(gradF, -1));
            }
            else
            {
                // Строим ККТ матрицу для активных ограничений
                int dim = n + k;
                double[,] kkt = new double[dim, dim];
                double[] rhs = new double[dim];
    
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        kkt[i, j] = h[i, j];
                    }
    
                    rhs[i] = -gradF[i];
                }
    
                for (int i = 0; i < k; i++)
                {
                    int cIdx = activeSet[i];
                    for (int j = 0; j < n; j++)
                    {
                        kkt[n + i, j] = jacG[cIdx, j];
                        kkt[j, n + i] = jacG[cIdx, j];
                    }
    
                    rhs[n + i] = -gVals[cIdx];
                }
    
                double[] sol = SolveLinearSystem(kkt, rhs);
                Array.Copy(sol, d, n);
                for (int i = 0; i < k; i++)
                {
                    lambda[activeSet[i]] = Math.Max(0, sol[n + i]);
                }
            }
    
            return d;
        }
    
        /// <summary>
        /// Одномерный поиск (Line Search) Армихо по штрафной функции Лагранжа
        /// </summary>
        private static double LineSearch(ObjectiveFunc f, ConstraintsFunc g, double[] x, double[] d, double[] gradF, double[] gVals)
        {
            double alpha = 1.0;
            const double RHO = 0.5; // Коэффициент уменьшения шага
            const double C = 0.1;   // Параметр Армихо
            int n = x.Length;
    
            // Вес штрафа за нарушение ограничений
            const double SIGMA = 10.0;
    
            // Текущий штраф
            double meritCurrent = f(x) + (SIGMA * ViolationSum(gVals));
    
            // Направление спуска штрафной функции
            double dMerit = Dot(gradF, d);
    
            for (int i = 0; i < 20; i++) // Максимум 20 итераций поиска длины
            {
                double[] xTrial = new double[n];
                for (int j = 0; j < n; j++)
                {
                    xTrial[j] = x[j] + (alpha * d[j]);
                }
    
                double meritTrial = f(xTrial) + (SIGMA * ViolationSum(g(xTrial)));
    
                if (meritTrial <= meritCurrent + (C * alpha * dMerit))
                {
                    return alpha;
                }
    
                alpha *= RHO;
            }
    
            return alpha;
        }
    
        /// <summary>
        /// Обновление Квазиньютоновской матрицы Гессиана методом BFGS (модификация Пауэлла для SQP)
        /// </summary>
        private static void UpdateHessianBfgs(double[,] h, double[] x, double[] xNext, double[] gradF, double[] gradFNext, double[,] jacG, double[,] jacGNext, double[] lambda, int m, int n)
        {
            double[] s = new double[n];
            for (int i = 0; i < n; i++)
            {
                s[i] = xNext[i] - x[i];
            }
    
            // Изменение градиента функции Лагранжа (L = f + lambda * g)
            double[] y = new double[n];
            for (int i = 0; i < n; i++)
            {
                double gLNext = gradFNext[i];
                double gLCurr = gradF[i];
                for (int j = 0; j < m; j++)
                {
                    gLNext += lambda[j] * jacGNext[j, i];
                    gLCurr += lambda[j] * jacG[j, i];
                }
    
                y[i] = gLNext - gLCurr;
            }
    
            double[] hs = Multiply(h, s);
            double sHs = Dot(s, hs);
            double sy = Dot(s, y);
    
            // Модификация Пауэлла для обеспечения положительной определенности матрицы
            if (sy < 0.2 * sHs)
            {
                double theta = 0.8 * sHs / (sHs - sy);
                for (int i = 0; i < n; i++)
                {
                    y[i] = (theta * y[i]) + ((1.0 - theta) * hs[i]);
                }
    
                sy = Dot(s, y);
            }
    
            if (Math.Abs(sy) < 1e-10)
            {
                return;
            }
    
            // Формула обновления BFGS: H = H + (y*y^T)/sy - (Hs*Hs^T)/sHs
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    h[i, j] += (y[i] * y[j] / sy) - (hs[i] * hs[j] / sHs);
                }
            }
        }
    
        // --- Вспомогательные математические методы ---
    
        private static double ViolationSum(double[] gVals)
        {
            double sum = 0;
            foreach (double v in gVals)
            {
                if (v > 0)
                {
                    sum += v;
                }
            }
    
            return sum;
        }
    
        private static double Norm(double[] v) { return Math.Sqrt(Dot(v, v)); }
        private static double Dot(double[] a, double[] b) { double s = 0; for (int i = 0; i < a.Length; i++) { s += a[i] * b[i]; } return s; }
        private static double[] Multiply(double[] v, double s) { double[] r = new double[v.Length]; for (int i = 0; i < v.Length; i++) { r[i] = v[i] * s; } return r; }
        private static double[] Multiply(double[,] m, double[] v) { int rows = m.GetLength(0); int cols = m.GetLength(1); double[] r = new double[rows]; for (int i = 0; i < rows; i++) { for (int j = 0; j < cols; j++) { r[i] += m[i, j] * v[j]; } } return r; }
    
        // Простой метод исключения Гаусса для решения СЛАУ (KKT систем)
        private static double[] SolveLinearSystem(double[,] a, double[] b)
        {
            int n = b.Length;
            double[,] m = (double[,])a.Clone();
            double[] x = (double[])b.Clone();
    
            for (int i = 0; i < n; i++)
            {
                int max = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(m[k, i]) > Math.Abs(m[max, i]))
                    {
                        max = k;
                    }
                }
    
                for (int k = i; k < n; k++)
                {
                    (m[max, k], m[i, k]) = (m[i, k], m[max, k]);
                }
    
                (x[max], x[i]) = (x[i], x[max]);
                if (Math.Abs(m[i, i]) < 1e-12)
                {
                    continue;
                }
    
                for (int k = i + 1; k < n; k++)
                {
                    double factor = m[k, i] / m[i, i];
                    x[k] -= factor * x[i];
                    for (int j = i; j < n; j++)
                    {
                        m[k, j] -= factor * m[i, j];
                    }
                }
            }
    
            double[] res = new double[n];
            for (int i = n - 1; i >= 0; i--)
            {
                double sum = 0;
                for (int j = i + 1; j < n; j++)
                {
                    sum += m[i, j] * res[j];
                }
    
                if (Math.Abs(m[i, i]) > 1e-12)
                {
                    res[i] = (x[i] - sum) / m[i, i];
                }
            }
    
            return res;
        }
    }
}

