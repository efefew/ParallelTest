global Nh_streams Nc_streams Nq_i Nq_j Nl_i Nl_j;
global ThinS TcinS;
global FCPh FCPc FCPinh FCPinc;
global Thin Thout Tcin Tcout;
global Tcin_q Tcout_q Thin_q Thout_q;
global Qh Qc QcSUM QhSUM Qc_q_st Qh_q_st;
global CFJ_S CFI_S CFJ CFI;
global Beta_j Beta_i Alfai Alfaj;

global Nc_sec_u Nh_sec_u FCPinc_sec_u FCPinh_sec_u ThinS_sec_u TcinS_sec_u CFI_S_sec_u CFJ_S_sec_u Beta_j_sec_u Beta_i_sec_u;
j=1;
for cs=1:1:Nc_streams
Tcin_q(cs,1)=TcinS(cs);
    for q=1:Nq_j
        Qc_q_st(cs,q)=QcSUM(cs)*Beta_j(cs,q);
        Tcin_q(cs,q+1)=Tcin_q(cs,q)+Qc_q_st(cs,q)/FCPinc(cs);
        Tcout_q(cs,q)=Tcin_q(cs,q+1);
        for l=1:Nl_j
            Qc(j)=QcSUM(cs)*Beta_j(cs,q)*Alfaj(cs,q,l);
            FCPc(j)=FCPinc(cs)*Alfaj(cs,q,l);
            CFJ(j)=CFJ_S(cs); % переопределение переменных
            Tcin(j)=Tcin_q(cs,q);
            if FCPc(j)==0
                Tcout(j)=Tcin(j);
            else
                Tcout(j)=Tcin_q(cs,q)+Qc(j)/FCPc(j);
            end
            j=j+1;
        end
    end
end
for cs=1:1:Nc_sec_u
    Tcin_q(Nc_streams + cs,1)=TcinS_sec_u(cs);
    for q=1:Nq_j
        Qc_q_st(Nc_streams + cs,q)=QcSUM(Nc_streams + cs)*Beta_j_sec_u(cs,q);
        Tcin_q(Nc_streams + cs,q+1)=Tcin_q(Nc_streams + cs,q)+Qc_q_st(Nc_streams + cs,q)/FCPinc_sec_u(cs);
        Tcout_q(Nc_streams + cs,q)=Tcin_q(Nc_streams + cs,q+1);
        for l=1:Nl_j
            Qc(j)=QcSUM(Nc_streams + cs)*Beta_j_sec_u(cs,q)*Alfaj(Nc_streams + cs,q,l);
            FCPc(j)=FCPinc_sec_u(cs)*Alfaj(Nc_streams + cs,q,l);
            CFJ(j)=CFJ_S_sec_u(cs); % переопределение переменных
            Tcin(j)=Tcin_q(Nc_streams + cs,q);
            if FCPc(j)==0
                Tcout(j)=Tcin(j);
            else
                Tcout(j)=Tcin_q(Nc_streams + cs,q)+Qc(j)/FCPc(j);
            end
            j=j+1;
        end
    end
end
i=1;
for hs=1:1:Nh_streams
    Thin_q(hs,1)=ThinS(hs);
    for q=1:Nq_i
        Qh_q_st(hs,q)=QhSUM(hs)*Beta_i(hs,q);
        Thin_q(hs,q+1)=Thin_q(hs,q)-Qh_q_st(hs,q)/FCPinh(hs);
        Thout_q(hs,q)=Thin_q(hs,q+1);
        for l=1:Nl_i
        Qh(i)=QhSUM(hs)*Beta_i(hs,q)*Alfai(hs,q,l);
        FCPh(i)=FCPinh(hs)*Alfai(hs,q,l);
        CFI(i)=CFI_S(hs); % переопределение переменных  
        Thin(i)=Thin_q(hs,q);
        if FCPh(i)==0
           Thout(i)=Thin(i);
        else
           Thout(i)=Thin_q(hs,q)-Qh(i)/FCPh(i);
        end
        i=i+1;
        end
    end
end
for hs=1:1:Nh_sec_u
    Thin_q(Nh_streams + hs,1)=ThinS_sec_u(hs);
    for q=1:Nq_i
        Qh_q_st(Nh_streams + hs,q)=QhSUM(Nh_streams + hs)*Beta_i_sec_u(hs,q);
        Thin_q(Nh_streams + hs,q+1)=Thin_q(Nh_streams + hs,q)-Qh_q_st(Nh_streams + hs,q)/FCPinh_sec_u(hs);
        Thout_q(Nh_streams + hs,q)=Thin_q(Nh_streams + hs,q+1);
        for l=1:Nl_i
        Qh(i)=QhSUM(Nh_streams + hs)*Beta_i_sec_u(hs,q)*Alfai(Nh_streams + hs,q,l);
        FCPh(i)=FCPinh_sec_u(hs)*Alfai(Nh_streams + hs,q,l);
        CFI(i)=CFI_S_sec_u(hs); % переопределение переменных  
        Thin(i)=Thin_q(Nh_streams + hs,q);
        if FCPh(i)==0
           Thout(i)=Thin(i);
        else
           Thout(i)=Thin_q(Nh_streams + hs,q)-Qh(i)/FCPh(i);
        end
        i=i+1;
        end
    end
end