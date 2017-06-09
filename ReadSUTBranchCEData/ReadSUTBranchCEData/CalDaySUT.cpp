#include "stdafx.h"
#include "CalDaySUT.h"

namespace sutnamespace
{
#pragma unmanaged
#define IGREG (15+31L*(10+12L*1582))
#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))
	CalDaySUT::CalDaySUT()
	{
	}
	CalDaySUT::~CalDaySUT()
	{
	}
	int CalDaySUT::CalDay(int mm, int id, int iyyy, int* countarray)
	{
		long jul;
		int ja, jy = iyyy, jm;


		if _ST_INSTRUMENT_DECISION(jy == 0,0) return -1;

		if _ST_INSTRUMENT_DECISION(jy < 0,1) ++jy;

		if _ST_INSTRUMENT_DECISION(mm > 2,2) {
			jm = mm + 1;
		}
		else {
			--jy;
			jm = mm + 13;
		}

		jul = (long)(floor(365.25*jy) + floor(30.6001*jm) + id + 1720995);

		if _ST_INSTRUMENT_DECISION(id + 31L * (mm + 12L * iyyy) >= IGREG,3) {
			ja = (int)(0.01*jy);
			jul += 2 - ja + (int)(0.25*ja);
		}
		jul = (jul + 1) % 7;

		if _ST_INSTRUMENT_DECISION(jul == 0,4)
			return 0;
		else if _ST_INSTRUMENT_DECISION(jul == 1,5)
			return 1;
		else if _ST_INSTRUMENT_DECISION(jul == 2,6)
			return 2;
		else if _ST_INSTRUMENT_DECISION(jul == 3,7)
			return 3;
		else if _ST_INSTRUMENT_DECISION(jul == 4,8)
			return 4;
		else if _ST_INSTRUMENT_DECISION(jul == 5,9)
			return 5;
		else if _ST_INSTRUMENT_DECISION(jul == 6,10)
			return 6;

		return 0;

	}
}

