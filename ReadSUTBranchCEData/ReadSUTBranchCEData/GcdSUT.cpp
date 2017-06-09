#include "stdafx.h"
#include "GcdSUT.h"

namespace sutnamespace
{
#pragma unmanaged  
#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))

	GcdSUT::GcdSUT()
	{
	}


	GcdSUT::~GcdSUT()
	{
	}

	int GcdSUT::gcd(int a, int b, int* countarray)
	{
		int tmp;

		if _ST_INSTRUMENT_DECISION(a < 0,0)
		{
			a = -a;
		}

		if (b < 0)
		{
			b = -b;
		}

		if _ST_INSTRUMENT_DECISION(a == 0 || b == 0,1)
		{
			return 0;
		}

		while _ST_INSTRUMENT_DECISION(b > 0,2)
		{
			tmp = a % b;
			a = b;
			b = tmp;
		}
	}
}

