#include "stdafx.h"
#include "bs.h"
#include <iostream>
using namespace std;
namespace sutnamespace
{
#pragma unmanaged  
#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))

	bs::bs()
	{
		try
		{
			int i = 0;
			int j = 2000;
			for (i = 0; i < 1000; i++)
			{
				data[i] = { i + 1, j - 1 };
			}
		}
		catch (int e)
		{
			cout << "An exception occurred. Exception Nr. " << e << '\n';
		}
	}


	bs::~bs()
	{
	}

	int bs::binarySearch(int x, int* countarray)
	{
		
		return bscore(x, countarray);
	}

	int bs::bscore(int x, int* countarray)
	{
	  int fvalue, mid, up, low ;

	  low = 0;
	  up = 999;
	  fvalue = -1 /* all data are positive */ ;
	  while _ST_INSTRUMENT_DECISION(low <= up,0) {
		mid = (low + up) >> 1;
		if _ST_INSTRUMENT_DECISION( data[mid].key == x,1) {  /*  found  */
		  up = low - 1;
		  fvalue = data[mid].value;
	#ifdef DEBUG
		printf("FOUND!!\n");
	#endif
		}
		else  /* not found */
		  if _ST_INSTRUMENT_DECISION( data[mid].key > x,2 ) 	{
		up = mid - 1;
	#ifdef DEBUG
		printf("MID-1\n");
	#endif
		  }
		  else   {
					low = mid + 1;
	#ifdef DEBUG
		printf("MID+1\n");
	#endif
		  }
	#ifdef DEBUG
		cnt1++;
	#endif
	  }
	#ifdef DEBUG
		printf("Loop Count : %d\n", cnt1);
	#endif
	  return fvalue;
	}	
}



