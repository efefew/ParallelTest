using System;
using System.IO;

public class InputModel
{
    // --- Контекст класса (вместо глобальных переменных MATLAB) ---
    public int[,] Assignment_ij;
    public int Nc_streams;
    public int Nh_streams;
    public int Nl_j;
    public int Nl_i;
    public int Nq_j;
    public int Nq_i;
    public double[] Qh;
    public double[] Qc;
    public double TOLERANCE_CONSTR;

    public double[] CFJ_S;
    public double[] CFI_S;
    public double[] CFI;
    public double CFUc;
    public double[] CFJ;
    public double CFUh;
    public int[,] v;
    public int[,] p;
    public double kQ1;
    public double kQ2;
    public double[] FCPc;
    public double[] FCPh;
    public double[] FCPinh;
    public double[] FCPinc;
    public double[] QhSUM;
    public double[] QcSUM;
    public double CA;
    public double C_gamma;
    public double year;
    public double delTij;
    public double Ccu;
    public double Chu;

    public double[] TcinS;
    public double[] ThinS;
    public double Thuin;
    public double Thuout;
    public double Tcuin;
    public double Tcuout;
    public double[] Tcin_q;
    public double[] Tcout_q;
    public double[] Thin_q;
    public double[] Thout_q;

    public double[,] dQreb;
    public double[,] dQcol;
    public double[,] dQhe;
    public double[,] Ahe;
    public double[,] Ac;
    public double[,] Ah;
    public double[,] Khe;
    public double[,] Kcol;
    public double[,] Kreb;
    public double[,] Ecol;
    public double[,] Ereb;

    public double[] ThoutS;
    public double[] TcoutS;
    public double[,] Beta_j;
    public double[,] Beta_i;
    public double[,,] Alfai;
    public double[,,] Alfaj;

    public int Nc_sec_u;
    public int Nh_sec_u;
    public double Cu_sec_u;

    // Дополнительные массивы, появившиеся в процессе расчета
    public double[] Thin;
    public double[] Thout;
    public double[] Tcin;
    public double[] Tcout;

    // --- Основные методы ---

    public double[,] Set(double var_, double kQ1_, double kQ2_, double year_,
                     int Nc_streams_, double[] TcinS_, double[] TcoutS_, double[] FCPinc_, double[] CFJ_S_, int Nq_j_, double[,] Beta_j_, int Nl_j_, double[] Gamma_j,
                     int Nh_streams_, double[] ThinS_, double[] ThoutS_, double[] FCPinh_, double[] CFI_S_, int Nq_i_, double[,] Beta_i_, int Nl_i_, double[] Gamma_i,
                     int Ncu_streams_, double Tcuin_, double Tcuout_, double CFUc_, double Ccu_,
                     int Nhu_streams_, double Thuin_, double Thuout_, double CFUh_, double Chu_,
                     double C_FIX_, double CA_, double C_gamma_,
                     double delTij_, double TOL_L1, double TOL_L2, double TOLERANCE_CONSTR_,
                     int Nc_sec_u_, int Nh_sec_u_, double Cu_sec_u_)
    {
        // ЭТАП 0: ВВОД ИСХОДНЫХ ДАННЫХ
        Cu_sec_u = Cu_sec_u_;
        Nc_sec_u = Nc_sec_u_;
        Nh_sec_u = Nh_sec_u_;

        kQ1 = kQ1_;
        kQ2 = kQ2_;
        year = year_;

        Nc_streams = Nc_streams_;
        TcinS = (double[])TcinS_.Clone();
        TcoutS = (double[])TcoutS_.Clone();
        FCPinc = (double[])FCPinc_.Clone();
        CFJ_S = (double[])CFJ_S_.Clone();
        Nq_j = Nq_j_;
        Beta_j = (double[,])Beta_j_.Clone();
        Nl_j = Nl_j_;

        Nh_streams = Nh_streams_;
        ThinS = (double[])ThinS_.Clone();
        ThoutS = (double[])ThoutS_.Clone();
        FCPinh = (double[])FCPinh_.Clone();
        CFI_S = (double[])CFI_S_.Clone();
        Nq_i = Nq_i_;
        Beta_i = (double[,])Beta_i_.Clone();
        Nl_i = Nl_i_;

        Tcuin = Tcuin_;
        Tcuout = Tcuout_;
        CFUc = CFUc_;
        Ccu = Ccu_;

        Thuin = Thuin_;
        Thuout = Thuout_;
        CFUh = CFUh_;
        Chu = Chu_;

        CA = CA_;
        C_gamma = C_gamma_;
        delTij = delTij_;
        TOLERANCE_CONSTR = TOLERANCE_CONSTR_;

        QcSUM = new double[Nc_streams];
        for (int cs = 0; cs < Nc_streams; cs++)
        {
            QcSUM[cs] = FCPinc[cs] * (TcoutS[cs] - TcinS[cs]);
        }

        QhSUM = new double[Nh_streams];
        for (int hs = 0; hs < Nh_streams; hs++)
        {
            QhSUM[hs] = FCPinh[hs] * (ThinS[hs] - ThoutS[hs]);
        }

        double[,] dQhe_UB = new double[Nh_streams, Nc_streams];
        double[,] U_he = new double[Nh_streams, Nc_streams];
        double[,] dQhe_UB_trans = new double[Nh_streams, Nc_streams];

        for (int hs = 0; hs < Nh_streams; hs++)
        {
            for (int cs = 0; cs < Nc_streams; cs++)
            {
                double val1 = QhSUM[hs];
                double val2 = QcSUM[cs];
                double minFCP = Math.Min(FCPinh[hs], FCPinc[cs]);
                double val3 = Math.Max(minFCP * (ThinS[hs] - TcinS[cs] - delTij), 0.0);

                dQhe_UB[hs, cs] = Math.Min(val1, Math.Min(val2, val3));
                U_he[hs, cs] = 1.0 / (1.0 / CFI_S[hs] + 1.0 / CFJ_S[cs]);
                dQhe_UB_trans[hs, cs] = dQhe_UB[hs, cs] / U_he[hs, cs];
            }
        }

        Alfai = new double[Nh_streams, Nq_i, Nl_i];
        for (int hs = 0; hs < Nh_streams; hs++)
        {
            for (int q = 0; q < Nq_i; q++)
            {
                for (int l = 0; l < Nl_i; l++)
                {
                    Alfai[hs, q, l] = Gamma_i[l];
                }
            }
        }

        Alfaj = new double[Nc_streams, Nq_j, Nl_j];
        for (int cs = 0; cs < Nc_streams; cs++)
        {
            for (int q = 0; q < Nq_j; q++)
            {
                for (int l = 0; l < Nl_j; l++)
                {
                    Alfaj[cs, q, l] = Gamma_j[l];
                }
            }
        }

        // ЭТАП 2: РЕШЕНИЕ ЗАДАЧИ MILP (подготовка матриц)
        System_Param();

        int sizeI = Nh_streams * Nq_i * Nl_i;
        int sizeJ = Nc_streams * Nq_j * Nl_j;

        v = new int[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        p = new int[sizeI, sizeJ];
        dQcol = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        dQreb = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        dQhe = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Ahe = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Ac = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Ah = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Khe = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Kcol = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Kreb = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Ecol = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];
        Ereb = new double[Math.Max(sizeI, sizeJ) + 1, Math.Max(sizeI, sizeJ) + 1];

        double[,] S = new double[Math.Max(sizeI, sizeJ), Math.Max(sizeI, sizeJ)];
        double[,] S1 = new double[sizeI, sizeJ];
        double[,] S2 = new double[sizeI, sizeJ];
        double[,] S3 = new double[sizeI, sizeJ];

        // ЭТАП 1: НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
        for (int i = 0; i < sizeI; i++)
        {
            for (int j = 0; j < sizeJ; j++)
            {
                if ((Thin[i] - Tcin[j]) < delTij)
                {
                    v[i, j] = 1;
                    dQcol[i, j] = Qh[i];
                    S1[i, j] = CalculateCooler(i, j, var_, dQcol[i, j], Thin[i], Tcuout, Thout[i], Tcuin);
                    dQreb[i, j] = Qc[j];
                    S2[i, j] = CalculateHeater(i, j, var_, dQreb[i, j], Thuout, Tcin[j], Thuin, Tcout[j]);
                    S[i, j] = S1[i, j] + S2[i, j];
                    Ahe[i, j] = 0;
                    dQhe[i, j] = 0;
                    Khe[i, j] = 0;
                    S3[i, j] = 0;
                }
                else
                {
                    dQhe[i, j] = Math.Min(Qh[i], Qc[j]);
                    double Tcoutm = Tcin[j] + (FCPc[j] != 0 ? (dQhe[i, j] / FCPc[j]) : 0);
                    double Thoutm = Thin[i] - (FCPh[i] != 0 ? (dQhe[i, j] / FCPh[i]) : 0);
                    double R1 = Thoutm - Tcin[j];
                    double R2 = Thin[i] - Tcoutm;

                    if (R1 < delTij || R2 < delTij)
                    {
                        v[i, j] = 2;
                        if (R2 > R1)
                        {
                            Thoutm = delTij + Tcin[j];
                            dQhe[i, j] = FCPh[i] * (Thin[i] - Thoutm);
                            Tcoutm = Tcin[j] + (FCPc[j] != 0 ? (dQhe[i, j] / FCPc[j]) : 0);
                            S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcoutm, Thoutm, Tcin[j], false);
                            p[i, j] = 1;
                        }
                        else
                        {
                            Tcoutm = Thin[i] - delTij;
                            dQhe[i, j] = FCPc[j] * (Tcoutm - Tcin[j]);
                            Thoutm = Thin[i] - (FCPh[i] != 0 ? (dQhe[i, j] / FCPh[i]) : 0);
                            S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcoutm, Thoutm, Tcin[j], false);
                            p[i, j] = 2;
                        }
                        dQcol[i, j] = Qh[i] - dQhe[i, j];
                        dQreb[i, j] = Qc[j] - dQhe[i, j];
                        S2[i, j] = CalculateCooler(i, j, var_, dQcol[i, j], Thoutm, Tcuout, Thout[i], Tcuin);
                        S3[i, j] = CalculateHeater(i, j, var_, dQreb[i, j], Thuout, Tcoutm, Thuin, Tcout[j]);
                        S[i, j] = S1[i, j] + S2[i, j] + S3[i, j];
                    }
                    else if (Qh[i] + TOL_L1 < Qc[j])
                    {
                        v[i, j] = 3;
                        dQhe[i, j] = Qh[i];
                        Tcoutm = Tcin[j] + (FCPc[j] != 0 ? (dQhe[i, j] / FCPc[j]) : 0);
                        S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcoutm, Thout[i], Tcin[j], false);
                        dQreb[i, j] = Qc[j] - dQhe[i, j];
                        S2[i, j] = CalculateHeater(i, j, var_, dQreb[i, j], Thuout, Tcoutm, Thuin, Tcout[j]);
                        S[i, j] = S1[i, j] + S2[i, j];
                        dQcol[i, j] = 0;
                        Ecol[i, j] = 0;
                        Kcol[i, j] = 0;
                        Ac[i, j] = 0;
                        S3[i, j] = 0;
                    }
                    else if (Qh[i] > Qc[j] + TOL_L1)
                    {
                        v[i, j] = 4;
                        dQhe[i, j] = Qc[j];
                        Thoutm = Thin[i] - (FCPh[i] != 0 ? (dQhe[i, j] / FCPh[i]) : 0);
                        S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcout[j], Thoutm, Tcin[j], false);
                        dQcol[i, j] = Qh[i] - dQhe[i, j];
                        S2[i, j] = CalculateCooler(i, j, var_, dQcol[i, j], Thoutm, Tcuout, Thout[i], Tcuin);
                        S[i, j] = S1[i, j] + S2[i, j];
                        dQreb[i, j] = 0;
                        Ah[i, j] = 0;
                        Ereb[i, j] = 0;
                        Kreb[i, j] = 0;
                        S3[i, j] = 0;
                    }
                    else
                    {
                        v[i, j] = 5;
                        dQhe[i, j] = Qc[j];
                        S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcout[j], Thout[i], Tcin[j], false);
                        S[i, j] = Math.Abs(S1[i, j]);
                        dQreb[i, j] = 0;
                        Ah[i, j] = 0;
                        Ereb[i, j] = 0;
                        Kreb[i, j] = 0;
                        dQcol[i, j] = 0;
                        Ecol[i, j] = 0;
                        Kcol[i, j] = 0;
                        Ac[i, j] = 0;
                        S2[i, j] = 0;
                        S3[i, j] = 0;
                    }
                }
            }

            // Построение матрицы квадратного вида
            int maxDim = Math.Max(sizeI, sizeJ);
            if (sizeI < sizeJ)
            {
                double[] Ah_AUT = new double[sizeJ];
                double[] Ereb_AUT = new double[sizeJ];
                double[] Kreb_AUT = new double[sizeJ];
                double[] S_AUT = new double[sizeJ];
                double[] dQreb_AUT = new double[sizeJ];

                for (int j = 0; j < sizeJ; j++)
                {
                    double dt1 = Thuout - Tcin[j];
                    double dt2 = Thuin - Tcout[j];
                    double deltaTj = Math.Pow(dt1 * dt2 * (dt1 + dt2) / 2.0, 1.0 / 3.0);
                    double U = 1.0 / (1.0 / CFJ[j] + 1.0 / CFUh);

                    Ah_AUT[j] = Qc[j] / (deltaTj * U);
                    Ereb_AUT[j] = Qc[j] * Chu;
                    Kreb_AUT[j] = (CA * Math.Pow(Ah_AUT[j], C_gamma)) / year;
                    S_AUT[j] = Ereb_AUT[j] + Kreb_AUT[j];

                    if (Qc[j] == 0) S_AUT[j] = 0;
                    else S_AUT[j] /= Math.Pow(Qc[j], var_);

                    dQreb_AUT[j] = Qc[j];
                }

                for (int i = sizeI; i < sizeJ; i++)
                {
                    for (int j = 0; j < sizeJ; j++)
                    {
                        v[i, j] = 6;
                        Ah[i, j] = Ah_AUT[j];
                        Ereb[i, j] = Ereb_AUT[j];
                        Kreb[i, j] = Kreb_AUT[j];
                        S[i, j] = S_AUT[j];
                        dQreb[i, j] = dQreb_AUT[j];
                    }
                }
            }
            else if (sizeI > sizeJ)
            {
                double[] Ac_AUT = new double[sizeI];
                double[] Ecol_AUT = new double[sizeI];
                double[] Kcol_AUT = new double[sizeI];
                double[] S_AUT = new double[sizeI];
                double[] dQcol_AUT = new double[sizeI];

                for (int i = 0; i < sizeI; i++)
                {
                    double dt1 = Thin[i] - Tcuout;
                    double dt2 = Thout[i] - Tcuin;
                    double deltaTi = Math.Pow(dt1 * dt2 * (dt1 + dt2) / 2.0, 1.0 / 3.0);
                    double U = 1.0 / (1.0 / CFI[i] + 1.0 / CFUc);

                    Ac_AUT[i] = Qh[i] / (deltaTi * U);
                    Ecol_AUT[i] = Qh[i] * Ccu;
                    Kcol_AUT[i] = (CA * Math.Pow(Ac_AUT[i], C_gamma)) / year;
                    S_AUT[i] = Ecol_AUT[i] + Kcol_AUT[i];

                    if (Qh[i] == 0) S_AUT[i] = 0;
                    else S_AUT[i] /= Math.Pow(Qh[i], var_);

                    dQcol_AUT[i] = Qh[i];
                }

                for (int i = 0; i < sizeI; i++)
                {
                    for (int j = sizeJ; j < sizeI; j++)
                    {
                        v[i, j] = 7;
                        Ac[i, j] = Ac_AUT[i];
                        Ecol[i, j] = Ecol_AUT[i];
                        Kcol[i, j] = Kcol_AUT[i];
                        S[i, j] = S_AUT[i];
                        dQcol[i, j] = dQcol_AUT[i];
                    }
                }
            }

            // Вызов алгоритма Венгерского метода (Munkres)
            var munkresResult = Munkres(S);
            Assignment_ij = munkresResult.Assignment;
            double[,] Cost_ij = munkresResult.CostMatrix;

            // Поиск минимальной стоимости
            var minCostResult = FindMinCost(1, 25, TOL_L1, TOL_L2, var_);

            // ЭТАП 5: ВЫВОД РЕЗУЛЬТАТОВ В ВИДЕ МАТРИЦЫ
            double dQhe_RES = 0, dQcol_RES = 0, dQreb_RES = 0;
            int k = 0;
            int idxI = 0;

            // Результаты можно писать в динамический список списков или файл
            for (int hs = 0; hs < Nh_streams; hs++)
            {
                for (int qi = 0; qi < Nq_i; qi++)
                {
                    for (int li = 0; li < Nl_i; li++)
                    {
                        int idxJ = 0;
                        for (int cs = 0; cs < Nc_streams; cs++)
                        {
                            for (int qj = 0; qj < Nq_j; qj++)
                            {
                                for (int lj = 0; lj < Nl_j; lj++)
                                {
                                    if (Assignment_ij[idxI, idxJ] == 1)
                                    {
                                        // Здесь логика записи строки в Results(k, 1..20)
                                        dQhe_RES += dQhe[idxI, idxJ];
                                        dQcol_RES += dQcol[idxI, idxJ];
                                        dQreb_RES += dQreb[idxI, idxJ];
                                        k++;
                                    }
                                    idxJ++;
                                }
                            }
                        }
                        idxI++;
                    }
                }
            }

            double QcSUM_RES = 0;
            foreach (var val in QcSUM) QcSUM_RES += val;

            double QhSUM_RES = 0;
            foreach (var val in QhSUM) QhSUM_RES += val;

            double E_RES = Ccu * dQcol_RES + Chu * dQreb_RES;
            double K_RES = minCostResult.SUM_F - E_RES;
            double COST_RES = minCostResult.SUM_F;

            ExcelExport();
            return Cost_ij;
        }

public (double T, double SUM_F) FindMinCost(double E, int iterations, double TOL_L1, double TOL_L2, double var_)
    {
        int id = 1;
        var costRes = FindCost(0, TOL_L1, TOL_L2, var_);
        double T = costRes.T;
        double SUM_F = costRes.SUM_F;

        while (id < iterations && T >= E)
        {
            costRes = FindCost(SUM_F, TOL_L1, TOL_L2, var_);
            T = costRes.T;
            SUM_F = costRes.SUM_F;
            id++;
            Console.WriteLine($"Iteration T: {T}");
        }
        return (T, SUM_F);
    }

    public (double T, double SUM_F) FindCost(double SUM_F_old, double TOL_L1, double TOL_L2, double var_)
    {
        System_Param();
        int sizeI = Nh_streams * Nq_i * Nl_i;
        int sizeJ = Nc_streams * Nq_j * Nl_j;
        int maxSize = Math.Max(sizeI, sizeJ);

        double[,] S = new double[maxSize, maxSize];
        double[,] S1 = new double[sizeI, sizeJ];
        double[,] S2 = new double[sizeI, sizeJ];
        double[,] S3 = new double[sizeI, sizeJ];

        for (int i = 0; i < sizeI; i++)
        {
            for (int j = 0; j < sizeJ; j++)
            {
                if ((Thin[i] - Tcin[j]) < delTij)
                {
                    v[i, j] = 1;
                    dQcol[i, j] = Qh[i];
                    S1[i, j] = CalculateCooler(i, j, var_, dQcol[i, j], Thin[i], Tcuout, Thout[i], Tcuin);
                    dQreb[i, j] = Qc[j];
                    S2[i, j] = CalculateHeater(i, j, var_, dQreb[i, j], Thuout, Tcin[j], Thuin, Tcout[j]);
                    S[i, j] = S1[i, j] + S2[i, j];
                    Ahe[i, j] = 0; dQhe[i, j] = 0; Khe[i, j] = 0; S3[i, j] = 0;
                }
                else
                {
                    dQhe[i, j] = Math.Min(Qh[i], Qc[j]);
                    double Tcoutm = Tcin[j] + (FCPc[j] != 0 ? (dQhe[i, j] / FCPc[j]) : 0);
                    double Thoutm = Thin[i] - (FCPh[i] != 0 ? (dQhe[i, j] / FCPh[i]) : 0);
                    double R1 = Thoutm - Tcin[j];
                    double R2 = Thin[i] - Tcoutm;

                    if (R1 < delTij || R2 < delTij)
                    {
                        v[i, j] = 2;
                        if (R2 > R1)
                        {
                            Thoutm = delTij + Tcin[j];
                            dQhe[i, j] = FCPh[i] * (Thin[i] - Thoutm);
                            Tcoutm = Tcin[j] + (FCPc[j] != 0 ? (dQhe[i, j] / FCPc[j]) : 0);
                            S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcoutm, Thoutm, Tcin[j], false);
                            p[i, j] = 1;
                        }
                        else
                        {
                            Tcoutm = Thin[i] - delTij;
                            dQhe[i, j] = FCPc[j] * (Tcoutm - Tcin[j]);
                            Thoutm = Thin[i] - (FCPh[i] != 0 ? (dQhe[i, j] / FCPh[i]) : 0);
                            S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcoutm, Thoutm, Tcin[j], false);
                            p[i, j] = 2;
                        }
                        dQcol[i, j] = Qh[i] - dQhe[i, j];
                        dQreb[i, j] = Qc[j] - dQhe[i, j];
                        S2[i, j] = CalculateCooler(i, j, var_, dQcol[i, j], Thoutm, Tcuout, Thout[i], Tcuin);
                        S3[i, j] = CalculateHeater(i, j, var_, dQreb[i, j], Thuout, Tcoutm, Thuin, Tcout[j]);
                        S[i, j] = S1[i, j] + S2[i, j] + S3[i, j];
                    }
                    else if (Qh[i] + TOL_L1 < Qc[j])
                    {
                        v[i, j] = 3;
                        dQhe[i, j] = Qh[i];
                        Tcoutm = Tcin[j] + (FCPc[j] != 0 ? (dQhe[i, j] / FCPc[j]) : 0);
                        S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcoutm, Thout[i], Tcin[j], false);
                        dQreb[i, j] = Qc[j] - dQhe[i, j];
                        S2[i, j] = CalculateHeater(i, j, var_, dQreb[i, j], Thuout, Tcoutm, Thuin, Tcout[j]);
                        S[i, j] = S1[i, j] + S2[i, j];
                        dQcol[i, j] = 0; Ecol[i, j] = 0; Kcol[i, j] = 0; Ac[i, j] = 0; S3[i, j] = 0;
                    }
                    else if (Qh[i] > Qc[j] + TOL_L1)
                    {
                        v[i, j] = 4;
                        dQhe[i, j] = Qc[j];
                        Thoutm = Thin[i] - (FCPh[i] != 0 ? (dQhe[i, j] / FCPh[i]) : 0);
                        S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcout[j], Thoutm, Tcin[j], false);
                        dQcol[i, j] = Qh[i] - dQhe[i, j];
                        S2[i, j] = CalculateCooler(i, j, var_, dQcol[i, j], Thoutm, Tcuout, Thout[i], Tcuin);
                        S[i, j] = S1[i, j] + S2[i, j];
                        dQreb[i, j] = 0;
                        Ah[i, j] = 0;
                        Ereb[i, j] = 0;
                        Kreb[i, j] = 0;
                        S3[i, j] = 0;
                    }
                    else
                    {
                        v[i, j] = 5;
                        dQhe[i, j] = Qc[j];
                        S1[i, j] = CalculateRecuperator(i, j, var_, dQhe[i, j], Thin[i], Tcout[j], Thout[i], Tcin[j], false);
                        S[i, j] = Math.Abs(S1[i, j]);
                        dQreb[i, j] = 0;
                        Ah[i, j] = 0;
                        Ereb[i, j] = 0;
                        Kreb[i, j] = 0;
                        dQcol[i, j] = 0;
                        Ecol[i, j] = 0;
                        Kcol[i, j] = 0;
                        Ac[i, j] = 0;
                        S2[i, j] = 0;
                        S3[i, j] = 0;
                    }
                }
            }

            // Построение квадратной матрицы (аналогично первой части)
            if (sizeI < sizeJ)
            {
                for (int i = sizeI; i < sizeJ; i++)
                {
                    for (int j = 0; j < sizeJ; j++)
                    {
                        v[i, j] = 6;
                    }
                }
            }
            else if (sizeI > sizeJ)
            {
                for (int i = 0; i < sizeI; i++)
                {
                    for (int j = sizeJ; j < sizeI; j++)
                    {
                        v[i, j] = 7;
                    }
                }
            }

            var munkresResult = Munkres(S);
            Assignment_ij = munkresResult.Assignment;

            // ЭТАП 3: РЕШЕНИЕ ЗАДАЧИ NLP
            var nlpResult = HEN_Optim(dQhe, Qc, Qh);
            double SUM_F = nlpResult.SUM_F;
            double[] X = nlpResult.X;
            int k = 0;

            for (int i = 0; i < sizeI; i++) Qh[i] = X[k++];
            for (int j = 0; j < sizeJ; j++) Qc[j] = X[k++];

            return (SUM_F_old, SUM_F);
        }
    }
}