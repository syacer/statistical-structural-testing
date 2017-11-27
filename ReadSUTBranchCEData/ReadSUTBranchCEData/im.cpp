#include "im.h"
#include<iostream>
using namespace std;
namespace sutnamespace
{
#pragma unmanaged  
#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))

	imatrix::imatrix()
	{
		
	}
	imatrix::~imatrix()
	{
		
	}
	
	int imatrix::inverseMatrix(
		double x1, double x2, double x3, double x4,
		double x5, double x6, double x7, double x8,
		double x9, int* countarray)
	{
		a[0][0] = x1;
		a[0][1] = x2;
		a[0][2] = x3;
		a[1][0] = x4;
		a[1][1] = x5;
		a[1][2] = x6;
		a[2][0] = x7;
		a[2][1] = x8;
		a[2][2] = x9;
		minverse(countarray);
		return 0;
	}
	
	double imatrix:: minver_fabs(double n, int* countarray)
	{
	  double f;

	  if _ST_INSTRUMENT_DECISION(n>=0,0) f = n;
	  else f = -n;
	  return f;
	}

	int imatrix::minverse(int* countarray)
	{
		int i, j;
		double eps;

		eps = 1.0e-6;

		for(i = 0; _ST_INSTRUMENT_DECISION(i < 3,1); i++)
		  for(j = 0; _ST_INSTRUMENT_DECISION(j < 3,2); j++)
			aa[i][j] = a[i][j];

		minver(3, 3, eps,countarray);
		for(i = 0; _ST_INSTRUMENT_DECISION(i < 3,3); i++)
		  for(j = 0; _ST_INSTRUMENT_DECISION(j < 3,4); j++)
			a_i[i][j] = a[i][j];

		mmul(3, 3, 3, 3, countarray);
		return 0;
	}

	int  imatrix::mmul(int row_a, int col_a, int row_b, int col_b, int* countarray)
	{
		 int i, j, k, row_c, col_c;
		 double w;

		 row_c = row_a;
		 col_c = col_b;

		 if _ST_INSTRUMENT_DECISION(row_c < 1 || row_b < 1 || col_c < 1 || col_a != row_b,5) return(999);
		 for(i = 0; _ST_INSTRUMENT_DECISION(i < row_c,6); i++)
		 {
		   for(j = 0;  _ST_INSTRUMENT_DECISION(j < col_c,7); j++)
			 {
			   w = 0.0;
			   for(k = 0; _ST_INSTRUMENT_DECISION(k < row_b,8); k++)
				w += a[i][k] * b[k][j];
			    c[i][j] = w;
			 }
		 }
		 return(0);

	}


	int imatrix::minver(int row, int col, double eps, int* countarray)
	{

		int work[500], i, j, k, r, iw, s, t, u, v;
		double w, wmax, pivot, api, w1;

		if _ST_INSTRUMENT_DECISION(row < 2 || row > 500 || eps <= 0.0,9) return(999);
		w1 = 1.0;
		for(i = 0; _ST_INSTRUMENT_DECISION(i < row,10); i++)
		  work[i] = i;
		for(k = 0; _ST_INSTRUMENT_DECISION(k < row,11); k++)
		  {
			wmax = 0.0;
			for(i = k; _ST_INSTRUMENT_DECISION(i < row,12); i++)
			  {
			w = minver_fabs(a[i][k],countarray);
			if _ST_INSTRUMENT_DECISION(w > wmax,13)
			  {
				wmax = w;
				r = i;
			  }
			  }
			pivot = a[r][k];
			api = minver_fabs(pivot,countarray);
			if _ST_INSTRUMENT_DECISION(api <= eps,14)
			  {
			det = w1;
			return(1);
			  }
			w1 *= pivot;
			u = k * col;
			v = r * col;
			if _ST_INSTRUMENT_DECISION(r != k,15)
			  {
				w1 = -w;
				iw = work[k];
				work[k] = work[r];
				work[r] = iw;
				for(j = 0; _ST_INSTRUMENT_DECISION(j < row,16); j++)
				  {
					s = u + j;
					t = v + j;
					w = a[k][j];
					a[k][j] = a[r][j];
					a[r][j] = w;
				  }
			  }
			for(i = 0; _ST_INSTRUMENT_DECISION(i < row,17); i++)
			  a[k][i] /= pivot;
			for(i = 0; _ST_INSTRUMENT_DECISION(i < row,18); i++)
			  {
			if _ST_INSTRUMENT_DECISION(i != k,19)
			  {
				v = i * col;
				s = v + k;
				w = a[i][k];
				if _ST_INSTRUMENT_DECISION(w != 0.0,20)
				  {
				for(j = 0; _ST_INSTRUMENT_DECISION(j < row,21); j++)
				  if _ST_INSTRUMENT_DECISION(j != k,22) a[i][j] -= w * a[k][j];
				a[i][k] = -w / pivot;
				  }
			  }
			  }
			a[k][k] = 1.0 / pivot;
		  }
		for(i = 0; _ST_INSTRUMENT_DECISION(i < row,23); i++)
		  {
			while _ST_INSTRUMENT_DECISION(1,24)
			{
				k = work[i];
				if _ST_INSTRUMENT_DECISION(k == i,25) break;
				iw = work[k];
				work[k] = work[i];
				work[i] = iw;
				for(j = 0; _ST_INSTRUMENT_DECISION(j < row,26); j++)
				  {
					u = j * col;
					s = u + i;
					t = u + k;
					w = a[k][i];
					a[k][i] = a[k][k];
					a[k][k] = w;
				  }
			}
		  }
		det = w1;
		return(0);	
	}

}





