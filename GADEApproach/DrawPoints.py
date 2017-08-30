import matplotlib.pyplot as plt

def func1():
	x=[-1.06587606416874,1.3931576658341,1.02024995527332,1.00422937800982,0.93680538782034]
	y=[2.19421687067302,-0.420486794473341,0.0231488153337336,-0.0254304671655038,0.0161758222826959]
	plt.plot(x, y, 'ro')
	plt.axis([-1.5, 2.5, -1.5, 2.5])
	plt.show()	

def func2():
	count = 0
	x=[]
	y=[]
	with open("C:/Users/shiya/Desktop/record/amatrix.txt") as fp:
		for line in fp:
			if count % 7 == 0 and count != 0:
				print "S"+ str(count) 
				lineOfProbs = line
				print lineOfProbs
				probsStr = lineOfProbs.split(" ")
				count2 = 0
				print probsStr
				for probStr in probsStr:
					print count2
					if (count2 == 0):
						print probStr
						x.append(float(probStr))
					if (count2 == 1):
						print probStr
						y.append(float(probStr))
					count2 = count2 + 1

			count = count + 1
	
	plt.plot(x, y, 'ro')
	plt.axis([-0.5, 1.5, -0.5, 1.5])
	plt.show()
	
func1()