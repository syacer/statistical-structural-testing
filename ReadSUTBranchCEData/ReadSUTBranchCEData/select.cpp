/*************************************************************************/
/*                                                                       */
/*   SNU-RT Benchmark Suite for Worst Case Timing Analysis               */
/*   =====================================================               */
/*                              Collected and Modified by S.-S. Lim      */
/*                                           sslim@archi.snu.ac.kr       */
/*                                         Real-Time Research Group      */
/*                                        Seoul National University      */
/*                                                                       */
/*                                                                       */
/*        < Features > - restrictions for our experimental environment   */
/*                                                                       */
/*          1. Completely structured.                                    */
/*               - There are no unconditional jumps.                     */
/*               - There are no exit from loop bodies.                   */
/*                 (There are no 'break' or 'return' in loop bodies)     */
/*          2. No 'switch' statements.                                   */
/*          3. No 'do..while' statements.                                */
/*          4. Expressions are restricted.                               */
/*               - There are no multiple expressions joined by 'or',     */
/*                'and' operations.                                      */
/*          5. No library calls.                                         */
/*               - All the functions needed are implemented in the       */
/*                 source file.                                          */
/*                                                                       */
/*                                                                       */
/*************************************************************************/
/*                                                                       */
/*  FILE: select.c                                                       */
/*  SOURCE : Numerical Recipes in C - The Second Edition                 */
/*                                                                       */
/*  DESCRIPTION :                                                        */
/*                                                                       */
/*     A function to select the Nth largest number in the floating poi-  */
/*     nt array arr[].                                                   */
/*     The parameters to function select are k and n. Then the function  */
/*     selects k-th largest number out of n original numbers.            */
/*                                                                       */
/*  REMARK :                                                             */
/*                                                                       */
/*  EXECUTION TIME :                                                     */
/*                                                                       */
/*                                                                       */
/*************************************************************************/
#include "stdafx.h"
#include "select.h"

namespace sutnamespace
{
	#pragma unmanaged  
	#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))	
	#define SWAP(a,b) temp=(a);(a)=(b);(b)=temp;
	
	select::select()
	{	
	  arr[0] = 5; arr[1] = 4; arr[2] = 10.3; arr[3] = 1.1;
	  arr[4] = 5.7; arr[5] = 100; arr[6] = 231; arr[7] = 111;
	  arr[8] = 49.5; arr[9] = 99; arr[10] = 10; arr[11] = 150;
	  arr[12] = 222.22; arr[13] = 101;
	  arr[14] = 77; arr[15] = 44; arr[16] = 35;  arr[17] = 20.54;  arr[18] = 99.99;
	  arr[19] = 888.88;
	}
	select::~select()
	{		
	}
	int select::Select(int k, int n, int* countarray)
	{
		return selectcore(k,n,countarray);
	}
	float select::selectcore(unsigned long k, unsigned long n, int* countarray)
	{
		unsigned long i,ir,j,l,mid;
		float a,temp;
		int flag, flag2;

		l=1;
		ir=n;
		flag = flag2 = 0; 
		while _ST_INSTRUMENT_DECISION(!flag,0) {
			if _ST_INSTRUMENT_DECISION(ir <= l+1,1) {
				if _ST_INSTRUMENT_DECISION(ir == l+1,2) 
				  if _ST_INSTRUMENT_DECISION(arr[ir] < arr[l],3) {
					SWAP(arr[l],arr[ir])
					  }
				flag = 1;
			} else if _ST_INSTRUMENT_DECISION(!flag,4) {
				mid=(l+ir) >> 1;
				SWAP(arr[mid],arr[l+1])
				if _ST_INSTRUMENT_DECISION(arr[l+1] > arr[ir],5) {
					SWAP(arr[l+1],arr[ir])
				}
				if _ST_INSTRUMENT_DECISION(arr[l] > arr[ir],6) {
					SWAP(arr[l],arr[ir])
				}
				if _ST_INSTRUMENT_DECISION(arr[l+1]> arr[l],7) {
					SWAP(arr[l+1],arr[l])
				}
				i=l+1;
				j=ir;
				a=arr[l];
				while _ST_INSTRUMENT_DECISION(!flag2,8) {
					i++; 
					while _ST_INSTRUMENT_DECISION(arr[i] < a,9) i++;
					j--; 
					while _ST_INSTRUMENT_DECISION(arr[j] > a,10) j--;
					if _ST_INSTRUMENT_DECISION(j < i,11) flag2 = 1;
					if _ST_INSTRUMENT_DECISION(!flag2,12) SWAP(arr[i],arr[j]);
					
				}
				arr[l]=arr[j];
				arr[j]=a;
				if _ST_INSTRUMENT_DECISION(j >= k,13) ir=j-1;
				if _ST_INSTRUMENT_DECISION(j <= k,14) l=i;
			}
		}
		return arr[k];
	}
}