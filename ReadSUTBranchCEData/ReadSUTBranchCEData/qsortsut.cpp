#include "stdafx.h"
#include "qsortsut.h"
#include <signal.h>
#include <iostream>
using namespace std;
namespace sutnamespace
{
#pragma unmanaged  
#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))
#define SWAP(a,b) temp=(a);(a)=(b);(b)=temp;
#define M 7
#define NSTACK 50

	qsortSUT::qsortSUT()
	{
		
	}


	qsortSUT::~qsortSUT()
	{
	}

	int qsortSUT::quickSort(
		float x1, float x2, float x3, float x4,
		float x5, float x6, float x7, float x8,
		float x9, float x10, float x11, float x12,
		float x13, float x14, float x15, float x16,
		float x17, float x18, float x19, float x20,
		int* countarray)
	{
		arr[0] = x1;arr[1] = x2;arr[2] = x3;arr[3] = x4;arr[4] = x5;
		arr[5] = x6;arr[6] = x7;arr[7] = x8;arr[8] = x9;arr[9] = x10;
		arr[10] = x11;arr[11] = x12;arr[12] = x13;arr[13] = x14;arr[14] = x15;
		arr[15] = x16;arr[16] = x17;arr[17] = x18;arr[18] = x19;arr[19] = x20;	
		//signal(SIGINT,ALARMhandler(1));
		sort(20, countarray);
		return 0;
	}

	void qsortSUT::sort(unsigned long n, int* countarray)
	{
		unsigned long i,ir=n,j,k,l=1;
		int jstack=0;
		int flag;
		float a,temp;
		int istack[100];
		
		flag = 0;
		for (;;) {
			if _ST_INSTRUMENT_DECISION(ir-l < M,0) {
				for (j=l+1;_ST_INSTRUMENT_DECISION(j<=ir,1);j++) {
					a=arr[j];
					for (i=j-1;_ST_INSTRUMENT_DECISION(i>=l,2);i--) {
						if _ST_INSTRUMENT_DECISION(arr[i] <= a,3) break;
						arr[i+1]=arr[i];
					}
					arr[i+1]=a;
				}
				if _ST_INSTRUMENT_DECISION(jstack == 0,4) break;
				ir=istack[jstack--];
				l=istack[jstack--];
			} else {
				k=(l+ir) >> 1;
				SWAP(arr[k],arr[l+1])
				if _ST_INSTRUMENT_DECISION(arr[l] > arr[ir],5) {
					SWAP(arr[l],arr[ir])
				}
				if _ST_INSTRUMENT_DECISION(arr[l+1] > arr[ir],6) {
					SWAP(arr[l+1],arr[ir])
				}
				if _ST_INSTRUMENT_DECISION(arr[l] > arr[l+1],7) {
					SWAP(arr[l],arr[l+1])
				}
				i=l+1;
				j=ir;
				a=arr[l+1];
				for (;;) {
					i++; while _ST_INSTRUMENT_DECISION(arr[i] < a,8) i++;
					j--; while _ST_INSTRUMENT_DECISION(arr[j] > a,9) j--;
					if _ST_INSTRUMENT_DECISION(j < i,10) break;
					SWAP(arr[i],arr[j]);
				}
				arr[l+1]=arr[j];
				arr[j]=a;
				jstack += 2;

				if _ST_INSTRUMENT_DECISION(ir-i+1 >= j-l,11) {
					istack[jstack]=ir;
					istack[jstack-1]=i;
					ir=j-1;
				} else {
					istack[jstack]=j-1;
					istack[jstack-1]=l;
					l=i;
				}
			}
		}
	}
}
