classdef INPUT
    methods (Static)
    function[Cost_ij] = Set(var_, kQ1_, kQ2_, year_, ...
                 Nc_streams_, TcinS_, TcoutS_, FCPinc_, CFJ_S_, Nq_j_, Beta_j_, Nl_j_, Gamma_j, ...
                 Nh_streams_, ThinS_, ThoutS_, FCPinh_, CFI_S_, Nq_i_, Beta_i_, Nl_i_, Gamma_i, ...
                 Ncu_streams_, Tcuin_, Tcuout_, CFUc_, Ccu_, ...
                 Nhu_streams_, Thuin_, Thuout_, CFUh_, Chu_, ...
                 C_FIX_, CA_, C_gamma_, ..._
                 delTij_, TOL_L1, TOL_L2,TOLERANCE_CONSTR_, ...
                 Nc_sec_u_, Nh_sec_u_, Cu_sec_u_)
format long g
%%Ввод исходных данных
global Assignment_ij Nc_streams Nh_streams Nl_j Nl_i Nq_j Nq_i Qh Qc TOLERANCE_CONSTR;
global CFJ_S CFI_S CFI CFUc CFJ CFUh v kQ1 kQ2 FCPc FCPh FCPinh FCPinc QhSUM QcSUM CA C_gamma year delTij Ccu Chu;
global TcinS ThinS Thuin Thuout Tcuin Tcuout Tcin_q Tcout_q Thin_q Thout_q;
global dQreb dQcol dQhe Ahe Ac Ah Khe Kcol Kreb Ecol Ereb;
global ThoutS TcoutS;
global Beta_j Beta_i Alfai Alfaj

global Nc_sec_u Nh_sec_u Cu_sec_u;
% ЭТАП 0 ВВОД ИСХОДНЫХ ДАННЫХ

% Ввод параметров вторичных утилит

Cu_sec_u = Cu_sec_u_; % коэффициент корреляции
Nc_sec_u = Nc_sec_u_;%количество горячих потоков вторичных утилит
Nh_sec_u = Nh_sec_u_;%количество холодных потоков вторичных утилит

% ЭТАП 0 ВВОД ИСХОДНЫХ ДАННЫХ
kQ1=kQ1_;%0;
kQ2=kQ2_;%0;
year=year_;%1;
% Ввод параметров холодных потоков
Nc_streams=Nc_streams_;%4; %количество холодных потоков
TcinS = TcinS_;%[323 408 391 353]; %температура входных холодных потоков
TcoutS=TcoutS_;%[503 409 392 354]; %температура выходных холодных потоков
FCPinc=FCPinc_;%[49.1 18413.1 18498.4 16347.9]; %теплоёмкость входных холодных потоков
CFJ_S=CFJ_S_;%[0.72 1.91 1.76 1.84]; % водяной эквивалент
Nq_j = Nq_j_;%4; %число стадий
Beta_j = Beta_j_;%[0.3 0.1 0.5 0.1;
                 % 0.1 0.5 0.3 0.1; 
                 % 0.2 0.2 0.5 0.1;
                 % 0.2 0.2 0.5 0.1];
Nl_j = Nl_j_;%3; % число делений
%Gamma_j=[0.4 0.3 0.3];

% Ввод параметров горячих потоков
Nh_streams = Nh_streams_;%3;
ThinS = ThinS_;%[503 425 381]; % температура входных горячих потоков
ThoutS = ThoutS_;%[308 424 380]; % температура выходных горячих потоков
FCPinh = FCPinh_;%[66.4 33020 12870]; %% теплоёмкость входных горячих потоков
CFI_S = CFI_S_;%[0.81 1.78 1.62]; % водяной эквивалент
Nq_i = Nq_i_;%4; % число стадий
Beta_i=Beta_i_;%[0.1 0.2 0.6 0.1;
               % 0.4 0.2 0.3 0.1;
               % 0.3 0.1 0.4 0.2];
Nl_i=Nl_i_;%4; % число делений
%Gamma_i=[0.1 0.2 0.6 0.1];

% Ввод параметров холодных внешних энергоносителей
Ncu_streams = Ncu_streams_;
Tcuin = Tcuin_;%303; % температура входного потока холодной утилиты
Tcuout = Tcuout_;%315; % температура выходного потока холодной утилиты
CFUc=CFUc_;%1; % коэффициенты теплопередачи холодных утилит
Ccu=Ccu_;%Ccu=10; % стоимость холодной утилиты

% Ввод параметров горячих внешних энергоносителей
Nhu_streams = Nhu_streams_;
Thuin = Thuin_;%627; % температура входного потока горячей утилиты
Thuout = Thuout_;%626; % температура выходного потока горячей утилиты
CFUh = CFUh_;%2.5; % коэффициенты теплоотдачи горячих утилит
Chu = Chu_;%100; % стоимость горячей утилиты

% Ввод стоимостных коэффициентов теплообменника
C_FIX=C_FIX_; % фиксированная стоимость
CA=CA_;%CA=380; % стоимостной коэффициент теплобменника($)
C_gamma = C_gamma_;%C_gamma=0.65; % коэффициент корреляции
delTij=delTij_; %5; минимально допустимая разность температур
%TOL_L1=0.0000001; % точность декомпозиции
%TOL_L2=0.0000001; % точность процедуры агрегирования
TOLERANCE_CONSTR=TOLERANCE_CONSTR_;%0.0001
% Окончание ввода

for cs=1:1:Nc_streams
    QcSUM(cs)=FCPinc(cs)*(TcoutS(cs)-TcinS(cs)); %количество теплоты передаваемое холодному потоку
end
for hs=1:1:Nh_streams
    QhSUM(hs)=FCPinh(hs)*(ThinS(hs)-ThoutS(hs)); %количество теплоты отнимаемое от горячего потока
end
for hs=1:1:(Nh_streams) 
    for cs=1:1:(Nc_streams)
        dQhe_UB(hs,cs)=min([QhSUM(hs) QcSUM(cs) max([(min([FCPinh(hs),FCPinc(cs)]))*(ThinS(hs)-TcinS(cs)-delTij) 0])]);
        U_he(hs,cs)=1/(1/CFI_S(hs)+1/CFJ_S(cs));
        dQhe_UB_trans(hs,cs)=dQhe_UB(hs,cs)/U_he(hs,cs);
    end
end
% M_COL=min(dQhe_UB); % минимальный элемент в столбце
% M_STR=min(dQhe_UB,[],2); % минимальный элемент в строке
% B_COL=sort(dQhe_UB); % сортировка в столбце
% B_STR=sort(dQhe_UB,2); % сортировка в строке
% 
% for hs=1:1:Nh_streams
%     sum_Beta=0;
%     for q=1:(Nq_i-1)
%         if q==1
%         Beta_i(hs,q)=B_STR(hs,q)/QhSUM(hs);
%         else
%         Beta_i(hs,q)=(B_STR(hs,q)-B_STR(hs,(q-1)))/QhSUM(hs);    
%         end
%         sum_Beta=sum_Beta+Beta_i(hs,q);
%     end
%     Beta_i(hs,Nq_i)=1-sum_Beta;
% end
% for cs=1:1:Nc_streams
%      sum_Beta=0;
%      for q=1:(Nq_j-1)
%          if q==1
%          Beta_j(cs,q)=B_COL(q,cs)/QcSUM(cs);
%          else
%          Beta_j(cs,q)=(B_COL(q,cs)-B_COL((q-1),cs))/QcSUM(cs);
%          end
%          sum_Beta=sum_Beta+Beta_j(cs,q);
%      end
%      Beta_j(cs,Nq_j)=1-sum_Beta;
% end

for hs=1:1:(Nh_streams) 
    for q=1:Nq_i
        for l=1:Nl_i
                Alfai(hs,q,l)=Gamma_i(l);  
              % Alfai(hs,q,l)=Beta_i(hs,q);
        end
    end
end
for cs=1:1:(Nc_streams)
     for q=1:Nq_j
         for l=1:Nl_j
                Alfaj(cs,q,l)=Gamma_j(l);  
              % Alfai(cs,q,l)=Beta_j(hs,q);
         end
     end
end

% ЭТАП 2 РЕШЕНИЕ ЗАДАЧИ MILP
% для теста
        var = var_;
        System_param
        S=0;
        % ЭТАП 1 НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
        for i=1:1:Nh_streams*Nq_i*Nl_i
            for j=1:1:Nc_streams*Nq_j*Nl_j
                % ЭБСТ1 Случай без рекуперативного теплообменника
                if (Thin(i)-Tcin(j))<delTij
                    v(i,j)=1; % тип ЭБСТ 
                    dQcol(i,j)=Qh(i); % тепловая нагрузка
                    S1(i,j)= INPUT.CalculateCoooler(i, j, var, dQcol(i,j), Thin(i), Tcuout, Thout(i), Tcuin); % Расчет холодильника
                    dQreb(i,j)=Qc(j); % тепловая нагрузка
                    S2(i, j) = INPUT.CalculateHeater(i, j, var, dQreb(i,j), Thuout, Tcin(j), Thuin, Tcout(j)); % Расчет нагревателя     
                    S(i,j)=S1(i,j)+S2(i,j); % затраты на ЭБСТ1
                    Ahe(i,j)=0;
                    dQhe(i,j)=0;
                    Khe(i,j)=0;
                    S3(i,j)=0;
                else
                    dQhe(i,j)=min(Qh(i),Qc(j)); % выбор минимального количества теплоты, затраченного на охлаждение/нагревание
                    Tcoutm = Tcin(j) + (dQhe(i,j)/FCPc(j)) * (FCPc(j)~=0);% нахождение температуры промежуточного холодного потока
                    Thoutm = Thin(i) - (dQhe(i,j)/FCPh(i)) * (FCPh(i)~=0);% нахождение температуры промежуточного горячего потока
                    R1=Thoutm-Tcin(j); % разность промежуточного горячего и входного холодного
                    R2=Thin(i)-Tcoutm; % разность входного горячего и выходного холодного промежуточного
                    if (R1<delTij)||(R2<delTij) % если R1 или R2 меньше минимально допустимой разности температур

                       % ЭБСТ2 Полноструктурный блок
                       v(i,j)=2; % тип ЭБСТ
                       if (R2>R1) % ЭБСТ2 Случай 1
                          Thoutm=delTij+Tcin(j) ; % нахождение температуры промежуточного горячего потока                
                          dQhe(i,j)=FCPh(i)*(Thin(i)-Thoutm); % нагрузка рекуператора
                          Tcoutm = Tcin(j) + (dQhe(i,j)/FCPc(j)) * (FCPc(j)~=0);% нахождение температуры промежуточного холодного потока
                          S1(i, j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcoutm, Thoutm, Tcin(j), false);% Рассчет рекуператора
                          p(i,j)=1;
                       else % если R1<R2
                          Tcoutm=Thin(i)-delTij; % нахождение температуры промежуточного холодного потока
                          dQhe(i,j)=FCPc(j)*(Tcoutm-Tcin(j)); % тепловая нагрузка рекуператора
                          Thoutm = Thin(i) - (dQhe(i,j)/FCPh(i)) * (FCPh(i)~=0);% нахождение температуры промежуточного горячего потока
                          S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcoutm, Thoutm, Tcin(j), false);% Рассчет рекуператора
                          p(i,j)=2;
                       end
                          dQcol(i,j)=Qh(i)-dQhe(i,j); % тепловая нагрузка холодильника
                          dQreb(i,j)=Qc(j)-dQhe(i,j); % тепловая нагрузка нагревателя
                          S2(i, j) = INPUT.CalculateCoooler(i, j, var, dQcol(i, j), Thoutm, Tcuout, Thout(i), Tcuin);% Рассчет холодильника
                          S3(i, j) = INPUT.CalculateHeater(i, j, var, dQreb(i, j), Thuout, Tcoutm, Thuin, Tcout(j));% Рассчет нагревателя
                          S(i,j)=S1(i,j)+S2(i,j)+S3(i,j); % затраты на ЭБСТ2
                          % РЕШЕНИЕ ЗАДАЧИ NLP
        %                   I=i;
        %                   J=j;
        %                   IP=dQhe(i,j);
        %                   [x_EBST,SUM_EBST,Flag_EBST]=HEN_optim_EBST(IP);
        %                   S(i,j)=SUM_EBST;

                    elseif  Qh(i)+TOL_L1<Qc(j)  % если R1>delTij и R2>delTij
                          % ЭБСТ3 Блок с концевым нагревателем
                          v(i,j)=3; % тип ЭБСТ
                          dQhe(i,j)=Qh(i); % тепловая нагрузка рекуператора                  
                          Tcoutm = Tcin(j) + (dQhe(i,j)/FCPc(j)) * (FCPc(j)~=0);% нахождение температуры промежуточного холодного потока
                          S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcoutm, Thout(i), Tcin(j), false);% Рассчет рекуператора
                          dQreb(i,j)=Qc(j)-dQhe(i,j);% тепловая нагрузка нагревателя
                          S2(i,j) = INPUT.CalculateHeater(i, j, var, dQreb(i,j), Thuout, Tcoutm, Thuin, Tcout(j));% Рассчет нагревателя
                          S(i,j)=S1(i,j)+S2(i,j); % затраты на ЭБСТ3
                          dQcol(i,j)=0; % холодильник отсутствует
                          Ecol(i,j)=0;
                          Kcol(i,j)=0;
                          Ac(i,j)=0;
                          S3(i,j)=0;
                   elseif  Qh(i)>Qc(j)+TOL_L1  % если R1>delTij и R2>delTij
                          % ЭБСТ4 Блок с концевым холодильником
                          v(i,j)=4; % тип ЭБСТ
                          dQhe(i,j)=Qc(j);
                          Thoutm = Thin(i) - (dQhe(i,j)/FCPh(i)) * (FCPh(i)~=0);% нахождение температуры промежуточного горячего потока
                          S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcout(j), Thoutm, Tcin(j), false);% Рассчет рекуператора
                          dQcol(i,j)=Qh(i)-dQhe(i,j); % тепловая нагрузка холодильника
                          S2(i,j) = INPUT.CalculateCoooler(i, j, var, dQcol(i,j), Thoutm, Tcuout, Thout(i), Tcuin);% Рассчет холодильника
                          S(i,j)=S1(i,j)+S2(i,j); % затраты на данный тип ЭБСТ
                          dQreb(i,j)=0; % нагреватель отсутствует
                          Ah(i,j)=0;
                          Ereb(i,j)=0;
                          Kreb(i,j)=0;
                          S3(i,j)=0;
                      else
                           % ЭБСТ5 Блок без концевых теплообменников
                           v(i,j)=5; % тип ЭБСТ
                           dQhe(i,j)=Qc(j);
                           S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcout(j), Thout(i), Tcin(j), false);% Рассчет рекуператора
                           S(i,j)=abs(S1(i,j)); % затраты на данный тип ЭБСТ
                           dQreb(i,j)=0; % нагреватель отсутствует
                           Ah(i,j)=0;
                           Ereb(i,j)=0;
                           Kreb(i,j)=0;
                           dQcol(i,j)=0; % холодильник отсутствует
                           Ecol(i,j)=0;
                           Kcol(i,j)=0;
                           Ac(i,j)=0;
                           S2(i,j)=0;
                           S3(i,j)=0;
                    end
                 end
            end
         end

        % Построение матрицы квадратного вида
        if ((Nh_streams*Nq_i*Nl_i)<(Nc_streams*Nq_j*Nl_j))
            for j=1:1:Nc_streams*Nq_j*Nl_j
                % Расчет нагревателя
                % Среднелогарифмическая разность температур
                dt1=Thuout-Tcin(j);
                dt2=Thuin-Tcout(j);
                deltaTj=(dt1*dt2*(dt1+dt2)/2)^(1/3);
                U=1/(1/CFJ(j)+1/CFUh); % коэффициент теплопередачи
                Ah_AUT(j)=Qc(j)/(deltaTj*U); %% площадь нагревателя
                Ereb_AUT(j)=Qc(j)*Chu; % эксплуатационные затраты
                Kreb_AUT(j)=(CA*Ah_AUT(j)^C_gamma)/year; % капитальные затраты
                S_AUT(j)=Ereb_AUT(j)+Kreb_AUT(j); % суммарные затраты
                if Qc(j)==0
                    S_AUT(j)=0;
                else
                    S_AUT(j)=S_AUT(j)/(Qc(j)^var);
                end
                dQreb_AUT(j)=Qc(j); % тепловая нагрузка
                Ahe_AUT(j)=0;
                dQhe_AUT(j)=0;
                Khe_AUT(j)=0;
                Ac_AUT(j)=0;
                Ecol_AUT(j)=0;
                Kcol_AUT(j)=0;
                dQcol_AUT(j)=0;  
            end
            for i=(Nh_streams*Nq_i*Nl_i+1):1:(Nc_streams*Nq_j*Nl_j)
                v(i,j)=6; % тип ЭБСТ
                Ah=cat(1,Ah,Ah_AUT); %% площадь нагревателя
                Ereb=cat(1,Ereb,Ereb_AUT); % эксплуатационные затраты
                Kreb=cat(1,Kreb,Kreb_AUT); % капитальные затраты
                S=cat(1,S,S_AUT); % суммарные затраты
                dQreb=cat(1,dQreb,dQreb_AUT); % тепловая нагрузка
                Ahe=cat(1,Ahe,Ahe_AUT);
                dQhe=cat(1,dQhe,dQhe_AUT);
                Khe=cat(1,Khe,Khe_AUT);
                Ac=cat(1,Ac,Ac_AUT);
                Ecol=cat(1,Ecol,Ecol_AUT);
                Kcol=cat(1,Kcol,Kcol_AUT);
                dQcol=cat(1,dQcol,dQcol_AUT);
            end
        elseif (Nh_streams*Nq_i*Nl_i)>(Nc_streams*Nq_j*Nl_j)
            for i=1:1:Nh_streams*Nq_i*Nl_i 
                % Расчет холодильника   
                % Среднелогарифмическая разность температур
                dt1=Thin(i)-Tcuout;
                dt2=Thout(i)-Tcuin; 
                deltaTi=(dt1*dt2*(dt1+dt2)/2)^(1/3);
                U=1/(1/CFI(i)+1/CFUc);
                Ac_AUT(i)=Qh(i)/(deltaTi*U); % площадь холодильника
                Ecol_AUT(i)=Qh(i)*Ccu; % эксплуатационные затраты
                Kcol_AUT(i)=(CA*Ac_AUT(i)^C_gamma)/year; % капитальные затраты
                S_AUT(i)=Ecol_AUT(i)+Kcol_AUT(i); % суммарные затраты
                if Qh(i)==0
                    S_AUT(i)=0;
                else
                    S_AUT(i)=S_AUT(i)/(Qh(i)^var);
                end
                dQcol_AUT(i)=Qh(i); % тепловая нагрузка
                Ahe_AUT(i)=0;
                dQhe_AUT(i)=0;
                Khe_AUT(i)=0;
                Ah_AUT(i)=0;
                Ereb_AUT(i)=0;
                Kreb_AUT(i)=0;
                dQreb_AUT(i)=0; 
            end
            for j=(Nc_streams*Nq_j*Nl_j+1):1:Nh_streams*Nq_i*Nl_i
                v(i,j)=7; % тип ЭБСТ
                Ac=cat(2,Ac,Ac_AUT); % площадь холодильника
                Ecol=cat(2,Ac,Ecol_AUT); % эксплуатационные затраты
                Kcol=cat(2,Ac,Kcol_AUT); % капитальные затраты
                S=cat(2,Ac,S_AUT); % суммарные затраты
                dQcol=cat(2,Ac,dQcol_AUT); % тепловая нагрузка
                Ahe=cat(2,Ahe,Ahe_AUT);
                dQhe=cat(2,dQhe,dQhe_AUT);
                Khe=cat(2,Khe,Khe_AUT);
                Ah=cat(2,Ah,Ah_AUT);
                Ereb=cat(2,Ereb,Ereb_AUT);
                Kreb=cat(2,Kreb,Kreb_AUT);
                dQreb=cat(2,dQreb,dQreb_AUT);
            end
        end

        [Assignment_ij,Cost_ij]=munkres(S);
% для теста
 [T, SUM_F] = INPUT.FindMinCost(1, 25, TOL_L1, TOL_L2, var_);

 %ЭТАП 5 ВЫВОД РЕЗУЛЬТАТОВ В ВИДЕ МАТРИЦЫ
 dQhe_RES=0;
 dQcol_RES=0;
 dQreb_RES=0;
 k=1;
 i=1;
 for hs=1:1:(Nh_streams)
     for qi=1:Nq_i
          for li=1:Nl_i
              j=1;
              for cs=1:1:(Nc_streams)
                  for qj=1:Nq_j
                      for lj=1:Nl_j
                          if Assignment_ij(i,j)==1
                          Results(k,1)=i;
                          Results(k,2)=j;
                          Results(k,3)=cs;
                          Results(k,4)=qj;
                          Results(k,5)=lj;
                          Results(k,6)=hs;
                          Results(k,7)=qi;
                          Results(k,8)=li;
                          Results(k,9)=v(i,j);
                          Results(k,10)=dQhe(i,j);
                          Results(k,11)=dQcol(i,j);
                          Results(k,12)=dQreb(i,j);
                          Results(k,13)=Ahe(i,j);
                          Results(k,14)=Ac(i,j);
                          Results(k,15)=Ah(i,j);
                          Results(k,16)=Khe(i,j);
                          Results(k,17)=Kcol(i,j);
                          Results(k,18)=Kreb(i,j);
                          Results(k,19)=Ecol(i,j);
                          Results(k,20)=Ereb(i,j);
                          
                          % Итоговый результат по системе 
                          dQhe_RES=dQhe_RES+dQhe(i,j);
                          dQcol_RES=dQcol_RES+dQcol(i,j);
                          dQreb_RES=dQreb_RES+dQreb(i,j);
                          k=k+1;                       
                          end
                          j=j+1;
                      end
                  end
              end
              i=i+1;
         end
     end
 end
 QcSUM_RES=sum(QcSUM);
 QhSUM_RES=sum(QhSUM);
 E_RES=Ccu*dQcol_RES+Chu*dQreb_RES;
 K_RES=SUM_F-E_RES;
 COST_RES=SUM_F;
% Вывод результатов в файл EXCEL

 EXCEL()


 %                  if (dt1-dt2==0)
 %                    deltaTi=dt1; 
 %                  else
 %                    deltaTi=(dt1-dt2)/log(dt1/dt2);
 %                  end
    end
    function [] = Aggregation()
        % AggregationStage();
        % AggregationDivision();
    end
    function [] = AggregationStage()
        global Nh_streams Nq_i Nl_i Nc_streams Nq_i Nl_i;
        global dQcol Ac dQreb Ah;
        
        resQ = 0;
        resA = 0;
        resI = 0;
        resJ = 0;
        i=1;
        for hs=1:1:(Nh_streams)
            for qi=1:Nq_i
                 for li=1:Nl_i
                     j=1;
                     for cs=1:1:(Nc_streams)
                         for qj=1:Nq_j
                             for lj=1:Nl_j
                                 if Assignment_ij(i,j)==1
                                    if(dQcol(i,j) ~= 0 && Ac ~= 0)
                                        resQ = resQ + dQcol;
                                        resA = resA + Ac;

                                        dQcol(i,j) = 0;
                                        Ac(i,j) = 0;
                                    end
                                 end
                                 j=j+1;
                             end
                         end
                     end
                     i=i+1;
                 end
            end
        end
        for hs=1:1:(Nc_streams) 
            for q=1:Nq_i
                for l=1:Nl_i
                    
                end
            end
        end
    end
    function [] = AggregationDivision()
    end
    function[T, SUM_F] = FindMinCost(E, iterations, TOL_L1, TOL_L2, var)
        id = 1;
        [T, SUM_F] = INPUT.FindCost(0, TOL_L1, TOL_L2, var);
        while id < iterations && T >= E
            [T, SUM_F] = INPUT.FindCost(SUM_F, TOL_L1, TOL_L2, var);
            id = id + 1;
            T
        end
    end
    
    function [T, SUM_F] = FindCost(SUM_F_old, TOL_L1, TOL_L2, var)
        global Assignment_ij Nc_streams Nh_streams Nl_j Nl_i Nq_j Nq_i Qh Qc;
        global CFI CFUc CFJ CFUh v FCPc FCPh QhSUM QcSUM CA C_gamma year delTij Ccu Chu;
        global Thin Thout Tcin Tcout Thuin Thuout Tcuin Tcuout;
        global dQreb dQcol dQhe Ahe Ac Ah Khe Kcol Kreb Ecol Ereb;
        global Beta_j Beta_i Alfai Alfaj

        % Расчет параметров системы теплообмена
        % DerivMatrix
        System_param
        S=0;
        % ЭТАП 1 НАХОЖДЕНИЕ ОПТИМАЛЬНЫХ ОЦЕНОК НА ТЕПЛООБМЕН ПАРЫ ПОТОКОВ
        for i=1:1:Nh_streams*Nq_i*Nl_i
            for j=1:1:Nc_streams*Nq_j*Nl_j
                % ЭБСТ1 Случай без рекуперативного теплообменника
                if (Thin(i)-Tcin(j))<delTij
                    v(i,j)=1; % тип ЭБСТ 
                    dQcol(i,j)=Qh(i); % тепловая нагрузка
                    S1(i,j)= INPUT.CalculateCoooler(i, j, var, dQcol(i,j), Thin(i), Tcuout, Thout(i), Tcuin); % Расчет холодильника
                    dQreb(i,j)=Qc(j); % тепловая нагрузка
                    S2(i, j) = INPUT.CalculateHeater(i, j, var, dQreb(i,j), Thuout, Tcin(j), Thuin, Tcout(j)); % Расчет нагревателя     
                    S(i,j)=S1(i,j)+S2(i,j); % затраты на ЭБСТ1
                    Ahe(i,j)=0;
                    dQhe(i,j)=0;
                    Khe(i,j)=0;
                    S3(i,j)=0;
                else
                    dQhe(i,j)=min(Qh(i),Qc(j)); % выбор минимального количества теплоты, затраченного на охлаждение/нагревание
                    Tcoutm = Tcin(j) + (dQhe(i,j)/FCPc(j)) * (FCPc(j)~=0);% нахождение температуры промежуточного холодного потока
                    Thoutm = Thin(i) - (dQhe(i,j)/FCPh(i)) * (FCPh(i)~=0);% нахождение температуры промежуточного горячего потока
                    R1=Thoutm-Tcin(j); % разность промежуточного горячего и входного холодного
                    R2=Thin(i)-Tcoutm; % разность входного горячего и выходного холодного промежуточного
                    if (R1<delTij)||(R2<delTij) % если R1 или R2 меньше минимально допустимой разности температур

                       % ЭБСТ2 Полноструктурный блок
                       v(i,j)=2; % тип ЭБСТ
                       if (R2>R1) % ЭБСТ2 Случай 1
                          Thoutm=delTij+Tcin(j) ; % нахождение температуры промежуточного горячего потока                
                          dQhe(i,j)=FCPh(i)*(Thin(i)-Thoutm); % нагрузка рекуператора
                          Tcoutm = Tcin(j) + (dQhe(i,j)/FCPc(j)) * (FCPc(j)~=0);% нахождение температуры промежуточного холодного потока
                          S1(i, j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcoutm, Thoutm, Tcin(j), false);% Рассчет рекуператора
                          p(i,j)=1;
                       else % если R1<R2
                          Tcoutm=Thin(i)-delTij; % нахождение температуры промежуточного холодного потока
                          dQhe(i,j)=FCPc(j)*(Tcoutm-Tcin(j)); % тепловая нагрузка рекуператора
                          Thoutm = Thin(i) - (dQhe(i,j)/FCPh(i)) * (FCPh(i)~=0);% нахождение температуры промежуточного горячего потока
                          S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcoutm, Thoutm, Tcin(j), false);% Рассчет рекуператора
                          p(i,j)=2;
                       end
                          dQcol(i,j)=Qh(i)-dQhe(i,j); % тепловая нагрузка холодильника
                          dQreb(i,j)=Qc(j)-dQhe(i,j); % тепловая нагрузка нагревателя
                          S2(i, j) = INPUT.CalculateCoooler(i, j, var, dQcol(i, j), Thoutm, Tcuout, Thout(i), Tcuin);% Рассчет холодильника
                          S3(i, j) = INPUT.CalculateHeater(i, j, var, dQreb(i, j), Thuout, Tcoutm, Thuin, Tcout(j));% Рассчет нагревателя
                          S(i,j)=S1(i,j)+S2(i,j)+S3(i,j); % затраты на ЭБСТ2
                          % РЕШЕНИЕ ЗАДАЧИ NLP
        %                   I=i;
        %                   J=j;
        %                   IP=dQhe(i,j);
        %                   [x_EBST,SUM_EBST,Flag_EBST]=HEN_optim_EBST(IP);
        %                   S(i,j)=SUM_EBST;

                    elseif  Qh(i)+TOL_L1<Qc(j)  % если R1>delTij и R2>delTij
                          % ЭБСТ3 Блок с концевым нагревателем
                          v(i,j)=3; % тип ЭБСТ
                          dQhe(i,j)=Qh(i); % тепловая нагрузка рекуператора                  
                          Tcoutm = Tcin(j) + (dQhe(i,j)/FCPc(j)) * (FCPc(j)~=0);% нахождение температуры промежуточного холодного потока
                          S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcoutm, Thout(i), Tcin(j), false);% Рассчет рекуператора
                          dQreb(i,j)=Qc(j)-dQhe(i,j);% тепловая нагрузка нагревателя
                          S2(i,j) = INPUT.CalculateHeater(i, j, var, dQreb(i,j), Thuout, Tcoutm, Thuin, Tcout(j));% Рассчет нагревателя
                          S(i,j)=S1(i,j)+S2(i,j); % затраты на ЭБСТ3
                          dQcol(i,j)=0; % холодильник отсутствует
                          Ecol(i,j)=0;
                          Kcol(i,j)=0;
                          Ac(i,j)=0;
                          S3(i,j)=0;
                   elseif  Qh(i)>Qc(j)+TOL_L1  % если R1>delTij и R2>delTij
                          % ЭБСТ4 Блок с концевым холодильником
                          v(i,j)=4; % тип ЭБСТ
                          dQhe(i,j)=Qc(j);
                          Thoutm = Thin(i) - (dQhe(i,j)/FCPh(i)) * (FCPh(i)~=0);% нахождение температуры промежуточного горячего потока
                          S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcout(j), Thoutm, Tcin(j), false);% Рассчет рекуператора
                          dQcol(i,j)=Qh(i)-dQhe(i,j); % тепловая нагрузка холодильника
                          S2(i,j) = INPUT.CalculateCoooler(i, j, var, dQcol(i,j), Thoutm, Tcuout, Thout(i), Tcuin);% Рассчет холодильника
                          S(i,j)=S1(i,j)+S2(i,j); % затраты на данный тип ЭБСТ
                          dQreb(i,j)=0; % нагреватель отсутствует
                          Ah(i,j)=0;
                          Ereb(i,j)=0;
                          Kreb(i,j)=0;
                          S3(i,j)=0;
                      else
                           % ЭБСТ5 Блок без концевых теплообменников
                           v(i,j)=5; % тип ЭБСТ
                           dQhe(i,j)=Qc(j);
                           S1(i,j) = INPUT.CalculateRecuperator(i, j, var, dQhe(i,j), Thin(i), Tcout(j), Thout(i), Tcin(j), false);% Рассчет рекуператора
                           S(i,j)=abs(S1(i,j)); % затраты на данный тип ЭБСТ
                           dQreb(i,j)=0; % нагреватель отсутствует
                           Ah(i,j)=0;
                           Ereb(i,j)=0;
                           Kreb(i,j)=0;
                           dQcol(i,j)=0; % холодильник отсутствует
                           Ecol(i,j)=0;
                           Kcol(i,j)=0;
                           Ac(i,j)=0;
                           S2(i,j)=0;
                           S3(i,j)=0;
                    end
                 end
            end
         end

        % Построение матрицы квадратного вида
        if ((Nh_streams*Nq_i*Nl_i)<(Nc_streams*Nq_j*Nl_j))
            for j=1:1:Nc_streams*Nq_j*Nl_j
                % Расчет нагревателя
                % Среднелогарифмическая разность температур
                dt1=Thuout-Tcin(j);
                dt2=Thuin-Tcout(j);
                deltaTj=(dt1*dt2*(dt1+dt2)/2)^(1/3);
                U=1/(1/CFJ(j)+1/CFUh); % коэффициент теплопередачи
                Ah_AUT(j)=Qc(j)/(deltaTj*U); %% площадь нагревателя
                Ereb_AUT(j)=Qc(j)*Chu; % эксплуатационные затраты
                Kreb_AUT(j)=(CA*Ah_AUT(j)^C_gamma)/year; % капитальные затраты
                S_AUT(j)=Ereb_AUT(j)+Kreb_AUT(j); % суммарные затраты
                if Qc(j)==0
                    S_AUT(j)=0;
                else
                    S_AUT(j)=S_AUT(j)/(Qc(j)^var);
                end
                dQreb_AUT(j)=Qc(j); % тепловая нагрузка
                Ahe_AUT(j)=0;
                dQhe_AUT(j)=0;
                Khe_AUT(j)=0;
                Ac_AUT(j)=0;
                Ecol_AUT(j)=0;
                Kcol_AUT(j)=0;
                dQcol_AUT(j)=0;  
            end
            for i=(Nh_streams*Nq_i*Nl_i+1):1:(Nc_streams*Nq_j*Nl_j)
                v(i,j)=6; % тип ЭБСТ
                Ah=cat(1,Ah,Ah_AUT); %% площадь нагревателя
                Ereb=cat(1,Ereb,Ereb_AUT); % эксплуатационные затраты
                Kreb=cat(1,Kreb,Kreb_AUT); % капитальные затраты
                S=cat(1,S,S_AUT); % суммарные затраты
                dQreb=cat(1,dQreb,dQreb_AUT); % тепловая нагрузка
                Ahe=cat(1,Ahe,Ahe_AUT);
                dQhe=cat(1,dQhe,dQhe_AUT);
                Khe=cat(1,Khe,Khe_AUT);
                Ac=cat(1,Ac,Ac_AUT);
                Ecol=cat(1,Ecol,Ecol_AUT);
                Kcol=cat(1,Kcol,Kcol_AUT);
                dQcol=cat(1,dQcol,dQcol_AUT);
            end
        elseif (Nh_streams*Nq_i*Nl_i)>(Nc_streams*Nq_j*Nl_j)
            for i=1:1:Nh_streams*Nq_i*Nl_i 
                % Расчет холодильника   
                % Среднелогарифмическая разность температур
                dt1=Thin(i)-Tcuout;
                dt2=Thout(i)-Tcuin; 
                deltaTi=(dt1*dt2*(dt1+dt2)/2)^(1/3);
                U=1/(1/CFI(i)+1/CFUc);
                Ac_AUT(i)=Qh(i)/(deltaTi*U); % площадь холодильника
                Ecol_AUT(i)=Qh(i)*Ccu; % эксплуатационные затраты
                Kcol_AUT(i)=(CA*Ac_AUT(i)^C_gamma)/year; % капитальные затраты
                S_AUT(i)=Ecol_AUT(i)+Kcol_AUT(i); % суммарные затраты
                if Qh(i)==0
                    S_AUT(i)=0;
                else
                    S_AUT(i)=S_AUT(i)/(Qh(i)^var);
                end
                dQcol_AUT(i)=Qh(i); % тепловая нагрузка
                Ahe_AUT(i)=0;
                dQhe_AUT(i)=0;
                Khe_AUT(i)=0;
                Ah_AUT(i)=0;
                Ereb_AUT(i)=0;
                Kreb_AUT(i)=0;
                dQreb_AUT(i)=0; 
            end
            for j=(Nc_streams*Nq_j*Nl_j+1):1:Nh_streams*Nq_i*Nl_i
                v(i,j)=7; % тип ЭБСТ
                Ac=cat(2,Ac,Ac_AUT); % площадь холодильника
                Ecol=cat(2,Ac,Ecol_AUT); % эксплуатационные затраты
                Kcol=cat(2,Ac,Kcol_AUT); % капитальные затраты
                S=cat(2,Ac,S_AUT); % суммарные затраты
                dQcol=cat(2,Ac,dQcol_AUT); % тепловая нагрузка
                Ahe=cat(2,Ahe,Ahe_AUT);
                dQhe=cat(2,dQhe,dQhe_AUT);
                Khe=cat(2,Khe,Khe_AUT);
                Ah=cat(2,Ah,Ah_AUT);
                Ereb=cat(2,Ereb,Ereb_AUT);
                Kreb=cat(2,Kreb,Kreb_AUT);
                dQreb=cat(2,dQreb,dQreb_AUT);
            end
        end

        % ЭТАП 2 РЕШЕНИЕ ЗАДАЧИ MILP
        [Assignment_ij,Cost_ij]=munkres(S);

        % ЭТАП 3 РЕШЕНИЕ ЗАДАЧИ NLP
        [X,SUM_F,Flag]=HEN_optim(dQhe,Qc, Qh);

        % ЭТАП 4 НАХОЖДЕНИЕ НОВЫХ ПРИБЛИЖЕНИЙ ПО ЗАКРЕПЛЯЕМЫМ ПЕРЕМЕННЫМ
        k=1;
            for i=1:1:Nh_streams*Nq_i*Nl_i
                Qh(i)=X(k); % переприсвоение переменной
                k=k+1;
            end
            for j=1:1:Nc_streams*Nq_j*Nl_j
                Qc(j)=X(k); % переприсвоение переменной
                k=k+1;
            end
        j=1;
        i=1;
        for cs=1:1:Nc_streams
            Err_c(cs)=1;
            for q=1:Nq_j
                SUM_Qc(cs,q)=0;
                for l=1:Nl_j
                    SUM_Qc(cs,q)=SUM_Qc(cs,q)+Qc(j);
                    j=j+1;
                end
                Beta_j(cs,q)=SUM_Qc(cs,q)/QcSUM(cs); % коэффициенты распределения нагрузок по стадиям
                if Beta_j(cs,q)<TOL_L2
                    Beta_j(cs,q)=0;
                    SUM_Qc(cs,q)=0;
                end
                Err_c(cs)=Err_c(cs)-Beta_j(cs,q);
            end
        end
        for cs=1:1:Nc_streams
            if (Err_c(cs)~=0)
                for q=Nq_j:-1:1
                    if Beta_j(cs,q)>0
                        if Beta_j(cs,q)<Err_c(cs)
                            Beta_j(cs,q)=Beta_j(cs,q)+Err_c(cs);
                            SUM_Qc(cs,q)=Beta_j(cs,q)*QcSUM(cs);
                            break
                        elseif Beta_j(cs,q)>Err_c(cs)
                            Beta_j(cs,q)=Beta_j(cs,q)+Err_c(cs);
                            SUM_Qc(cs,q)=Beta_j(cs,q)*QcSUM(cs);
                            if Beta_j(cs,q)<0
                               Err_c(cs)=Beta_j(cs,q); 
                               Beta_j(cs,q)=0;
                               SUM_Qc(cs,q)=0;
                            else
                            break
                            end
                        end
                    end
                end
            end
        end        
        for hs=1:1:Nh_streams
            Err_h(hs)=1;
            for q=1:Nq_i
                SUM_Qh(hs,q)=0;
                for l=1:Nl_i
                    SUM_Qh(hs,q)=SUM_Qh(hs,q)+Qh(i);
                    i=i+1;
                end
                Beta_i(hs,q)= SUM_Qh(hs,q)/QhSUM(hs); % коэффициенты распределения нагрузок по стадиям 
                if Beta_i(hs,q)<TOL_L2
                    Beta_i(hs,q)=0;
                    SUM_Qh(hs,q)=0;
                end
                Err_h(hs)=Err_h(hs)-Beta_i(hs,q);
            end
        end
        for hs=1:1:Nh_streams
            if (Err_h(hs)~=0)
                for q=Nq_i:-1:1
                    if Beta_i(hs,q)>0
                        if Beta_i(hs,q)<Err_h(hs)
                            Beta_i(hs,q)=Beta_i(hs,q)+Err_h(hs);
                            SUM_Qh(hs,q)=Beta_i(hs,q)*QhSUM(hs);
                            break
                        elseif Beta_i(hs,q)>Err_h(hs)
                            Beta_i(hs,q)=Beta_i(hs,q)+Err_h(hs);
                            SUM_Qh(hs,q)=Beta_i(hs,q)*QhSUM(hs);
                            if Beta_i(hs,q)<0
                               Err_h(hs)=Beta_i(hs,q); 
                               Beta_i(hs,q)=0;
                               SUM_Qh(hs,q)=0;
                            else
                            break
                            end
                        end
                    end
                end
            end
        end   
        i=1;
        j=1;
        for cs=1:1:Nc_streams
            for q=1:Nq_j
                Err_lc(cs,q)=1;
                for l=1:Nl_j
                    if SUM_Qc(cs,q)==0
                        Alfaj(cs,q,l)=1/Nl_j;
                    else
                        Alfaj(cs,q,l)=Qc(j)/SUM_Qc(cs,q); % коэффициенты распределения нагрузок по ветвям
                    end
                    if Alfaj(cs,q,l)<TOL_L2
                        Alfaj(cs,q,l)=0;
                    end
                    j=j+1;
                    Err_lc(cs,q)=Err_lc(cs,q)-Alfaj(cs,q,l);
                end
            end
        end
        for cs=1:1:Nc_streams
            for q=1:Nq_j
              if (Err_lc(cs,q)~=0)
                for l=Nl_j:-1:1
                    if Alfaj(cs,q,l)>0
                        if Alfaj(cs,q,l)<Err_lc(cs,q)
                            Alfaj(cs,q,l)=Alfaj(cs,q,l)+Err_lc(cs,q);
                            break
                        elseif Alfaj(cs,q,l)>Err_lc(cs,q)
                            Alfaj(cs,q,l)=Alfaj(cs,q,l)+Err_lc(cs,q);
                            if Alfaj(cs,q,l)<0
                               Err_lc(cs,q)=Alfaj(cs,q,l); 
                               Alfaj(cs,q,l)=0;
                            else
                               break
                            end
                        end
                    end
                end
              end
            end
        end   

        for hs=1:1:Nh_streams
            for q=1:Nq_i
                Err_lh(hs,q)=1;
                for l=1:Nl_i
                    if SUM_Qh(hs,q)==0
                        Alfai(hs,q,l)=1/Nl_i;
                    else
                        Alfai(hs,q,l)= Qh(i)/SUM_Qh(hs,q); % коэффициенты распределения нагрузок по ветвям
                    end
                    if Alfai(hs,q,l)<TOL_L2
                        Alfai(hs,q,l)=0;
                    end
                    i=i+1;
                    Err_lh(hs,q)=Err_lh(hs,q)-Alfai(hs,q,l);
                end
            end
        end
        for hs=1:1:Nh_streams
            for q=1:Nq_i
              if (Err_lh(hs,q)~=0)
                for l=Nl_i:-1:1
                    if Alfai(hs,q,l)>0
                        if Alfai(hs,q,l)<Err_lh(hs,q)
                            Alfai(hs,q,l)=Alfai(hs,q,l)+Err_lh(hs,q);
                            break
                        elseif Alfai(hs,q,l)>Err_lh(hs,q)
                            Alfai(hs,q,l)=Alfai(hs,q,l)+Err_lh(hs,q);
                            if Alfai(hs,q,l)<0
                               Err_lh(hs,q)=Alfai(hs,q,l); 
                               Alfai(hs,q,l)=0;
                            else
                               break
                            end
                        end
                    end
                end
              end
            end
        end
        % Расчет параметров системы теплообмена
        System_param
        % Критерий останова
        T=abs(SUM_F_old-SUM_F);
        SUM_F_old=SUM_F;
    end
    
    function [summ] = CalculateRecuperator(i, j, var, dQ, T11, T12, T21, T22, isMax)% Расчет рекуператора
        global CFI CFJ Ahe Khe CA C_gamma year dQhe Nh_streams Nq_i Nl_i Nc_streams Nq_j Nl_j;
        global Nc_sec_u Nh_sec_u Cu_sec_u;
        deltaT = INPUT.AverageLogarithmicDeltaTemperature(T11, T12, T21, T22);
        U = 1 / (1 / CFI(i) + 1 / CFJ(j));
        Ahe(i, j) = dQ / (deltaT * U); % площадь теплообмена
        if ~isempty(isMax)
            isMax = false;
        end
        maxLength = max((Nh_streams-Nh_sec_u)*Nq_i*Nl_i,(Nc_streams-Nc_sec_u)*Nq_j*Nl_j);
        if (~isMax && i > (Nh_streams-Nh_sec_u)*Nq_i*Nl_i || j > (Nc_streams-Nc_sec_u)*Nq_j*Nl_j) || (isMax && i > maxLength || j > maxLength)
            if (~isMax && i > (Nh_streams-Nh_sec_u)*Nq_i*Nl_i && j > (Nc_streams-Nc_sec_u)*Nq_j*Nl_j) || (isMax && i > maxLength && j > maxLength)
                Cu_ = realmax;
            else
                Cu_ = Cu_sec_u;
            end
        else
            Cu_ = 0;
        end
        Ehe = dQ * Cu_; % эксплуатационные затраты
        Khe(i, j) = (CA * power(Ahe(i, j), C_gamma)) / year; % капитальные затраты
        summ = Khe(i, j) + Ehe;
        summ = summ / (dQhe(i,j)^var) * (dQhe(i,j)~=0);
    end
    
    function [summ] = CalculateHeater(i, j, var, dQ, T11, T12, T21, T22)% Расчет нагревателя
        global CFJ CFUh Ah Kreb Ereb CA C_gamma Chu year dQreb;
        deltaT = INPUT.AverageLogarithmicDeltaTemperature(T11, T12, T21, T22);
        U = 1 / (1 / CFJ(j) + 1 / CFUh);
        Ah(i, j) = dQ / (deltaT * U); % площадь нагревателя

        Ereb(i, j) = dQ * Chu; % эксплуатационные затраты
        Kreb(i, j) = (CA * power(Ah(i, j), C_gamma)) / year; % капитальные затраты
        summ = Ereb(i, j) + Kreb(i, j);
        summ = summ / (dQreb(i,j)^var) * (dQreb(i,j)~=0);
    end
    
    function [summ] = CalculateCoooler(i, j, var, dQ, T11, T12, T21, T22)% Расчет холодильника
        global CFI CFUc Ac Kcol Ecol CA C_gamma Ccu year dQcol;
        deltaT = INPUT.AverageLogarithmicDeltaTemperature(T11, T12, T21, T22);
        U = 1 / (1 / CFI(i) + 1 / CFUc);
        Ac(i, j) = dQ / (deltaT * U); % площадь холодильника

        Ecol(i, j) = dQ * Ccu; % эксплуатационные затраты
        Kcol(i, j) = (CA * power(Ac(i, j), C_gamma)) / year; % капитальные затраты
        summ = Ecol(i, j) + Kcol(i, j);
        summ = summ / (dQcol(i,j)^var) * (dQcol(i,j)~=0);
    end
    
    function [deltaT] = AverageLogarithmicDeltaTemperature(T11, T12, T21, T22)%Среднелогарифмическая разность температур
        dt1 = T11 - T12;
        dt2 = T21 - T22;
        deltaT = power(dt1 * dt2 * (dt1 + dt2) / 2, 1/3);
    end
    end
end