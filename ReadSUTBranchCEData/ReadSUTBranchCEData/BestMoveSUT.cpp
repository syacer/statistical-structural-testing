#include "stdafx.h"
#include "BestMoveSUT.h"

namespace sutnamespace
{
#pragma unmanaged  
#define _ST_INSTRUMENT_DECISION(decision,id) (decision ? (countarray[2*id] = 1, -1) : (countarray[2*id+1] = 1, 0))

	BestMoveSUT::BestMoveSUT()
	{
	}
	BestMoveSUT::~BestMoveSUT()
	{
	}

	int BestMoveSUT::BestMove(int white, int black, int* countarray)
	{
		int won[(1 << 9) - 1];
		int w;
		int wl;
		int whiteCount, blackCount;
		int whiteTemp, blackTemp;
		int bestMove;
		int i;
		int mw, pw, mb, pb;
		int whiteWins, blackWins;

		/* The possible moves in order of importance...
		*
		*  0 | 1 | 2
		* ---+---+---
		*  3 | 4 | 5
		* ---+---+---
		*  6 | 7 | 8
		*/
		int moves[] = { 4, 0, 2, 6, 8, 1, 3, 5, 7 };

		/* All 8 possible winning lines ... */
		int winningLines[] = {
			(1 << 0) | (1 << 1) | (1 << 2),
			(1 << 3) | (1 << 4) | (1 << 5),
			(1 << 6) | (1 << 7) | (1 << 8),
			(1 << 0) | (1 << 3) | (1 << 6),
			(1 << 1) | (1 << 4) | (1 << 7),
			(1 << 2) | (1 << 5) | (1 << 8),
			(1 << 0) | (1 << 4) | (1 << 8),
			(1 << 2) | (1 << 4) | (1 << 6) };

		/* Derive the winning positions. */
		for (w = 0; _ST_INSTRUMENT_DECISION(w < ((1 << 9) - 1), 0); w++) {
			won[w] = 0;
			for (wl = 0; _ST_INSTRUMENT_DECISION(wl < 8, 1); wl++) {
				if _ST_INSTRUMENT_DECISION((w & winningLines[wl]) == winningLines[wl], 2) {
					won[w] = -1;
					break;
				}
			}

		}

		if _ST_INSTRUMENT_DECISION(white & black, 3) {
			/* have same positions - not possible */
			return -1;
		}

		whiteCount = 0;
		blackCount = 0;
		for (whiteTemp = white, blackTemp = black; _ST_INSTRUMENT_DECISION(whiteTemp | blackTemp, 4); whiteTemp = whiteTemp >> 1, blackTemp = blackTemp >> 1) {
			whiteCount += (whiteTemp & 1);
			blackCount += (blackTemp & 1);
		}

		if _ST_INSTRUMENT_DECISION(((blackCount - whiteCount) > 2) || ((blackCount - whiteCount) < 0), 5) {
			/* someone's skipped a turn: it's white's turn so either they have the same number of
			* positions, or black has one more
			*/
			return -2;
		}

		for (w = 0; _ST_INSTRUMENT_DECISION(w < ((1 << 9) - 1), 6); w++) {
			if _ST_INSTRUMENT_DECISION((won[w]) && (((w & white) == w) || ((w & black) == w)), 7) {
				/* someone's already won */
				return -3;
			}
		}

		bestMove = -1;
		for (i = 0; _ST_INSTRUMENT_DECISION(i < 9, 8); i++) {
			whiteWins = 0; /* introduced to avoid multiple places where valid move is output */
			blackWins = 0; /* introduced to simulate nested continue in original java */
			mw = moves[i];
			if _ST_INSTRUMENT_DECISION(((white & (1 << mw)) == 0) && ((black & (1 << mw)) == 0), 9) {

				pw = white | (1 << mw);

				if _ST_INSTRUMENT_DECISION(won[pw], 10) {
					whiteWins = -1;
				}
				else {

					for (mb = 0; _ST_INSTRUMENT_DECISION(mb < 9, 11); mb++) {
						if _ST_INSTRUMENT_DECISION(((pw & (1 << mb)) == 0) && ((black & (1 << mb)) == 0), 12) {
							pb = black | (1 << mb);
							if _ST_INSTRUMENT_DECISION(won[pb], 13) {
								/* black wins, take another */
								blackWins = -1;
								break;
							}
						}
					}

				}

				if _ST_INSTRUMENT_DECISION(whiteWins, 14) {
					break;	/* already found winning move - break from outer loop */
				}

				if _ST_INSTRUMENT_DECISION(!blackWins, 15) {
					if _ST_INSTRUMENT_DECISION(bestMove == -1, 16) {
						bestMove = mw;
					}
				}

			}

		}

		if _ST_INSTRUMENT_DECISION(bestMove == -1, 17) {
			/* No move is totally satisfactory, try the first one that is open */
			for (i = 0; _ST_INSTRUMENT_DECISION(i < 9, 18); i++) {
				mw = moves[i];
				if _ST_INSTRUMENT_DECISION(((white & (1 << mw)) == 0) && ((black & (1 << mw)) == 0), 19) {
					bestMove = mw;
					break;
				}
			}
		}

		/* if no moves are open, must be a stalemate position */
		if _ST_INSTRUMENT_DECISION(bestMove == -1, 20)
			return -4;

		return bestMove;

	}
}

