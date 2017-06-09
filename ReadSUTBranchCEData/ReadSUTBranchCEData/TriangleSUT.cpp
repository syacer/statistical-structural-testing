#include "stdafx.h"
#include "TriangleSUT.h"

namespace sutnamespace
{
#pragma unmanaged  
#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))
	TriangleSUT::TriangleSUT()
	{
		return;
	}

	int TriangleSUT::triang(int i, int j, int k, int *countarray)
	{
		int tri;
		
		if _ST_INSTRUMENT_DECISION((i <= 0) || (j <= 0) || (k <= 0), 0) return 4;

		tri = 0;

		if _ST_INSTRUMENT_DECISION(i == j,1) tri += 1;
		if _ST_INSTRUMENT_DECISION(i == k, 2) tri += 2;
		if _ST_INSTRUMENT_DECISION(j == k, 3) tri += 3;
		if _ST_INSTRUMENT_DECISION(tri == 0,4)
		{
			if _ST_INSTRUMENT_DECISION((i + j <= k) || (j + k <= i) || (i + k <= j),5)
				tri = 4;
			else
				tri = 1;
			return -1;
		}

		if _ST_INSTRUMENT_DECISION(tri > 3,6) tri = 3;
		else if _ST_INSTRUMENT_DECISION((tri == 1) && (i + j > k),7) tri = 2;
		else if _ST_INSTRUMENT_DECISION((tri == 2) && (i + k > j),8) tri = 2;
		else if _ST_INSTRUMENT_DECISION((tri == 3) && (j + k > i),9) tri = 2;
		else tri = 4;

		if _ST_INSTRUMENT_DECISION(tri == 1,10)	return 1;
		else if _ST_INSTRUMENT_DECISION(tri == 2,11)	return 2;
		else if _ST_INSTRUMENT_DECISION(tri == 3,12)	return 3;
		else if _ST_INSTRUMENT_DECISION(tri == 4,13)	return 4;

		return 0;
	}
}

