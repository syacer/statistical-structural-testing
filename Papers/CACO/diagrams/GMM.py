import matplotlib.pyplot as plt
import numpy as np
import matplotlib.mlab as mlab
import math

mu = 0
variance = 1
sigma = math.sqrt(variance)
x = np.linspace(-20, 60, 400)

plt.plot(x,0.3*mlab.normpdf(x, 1, 5) + 0.2*mlab.normpdf(x, 20, 2) + 0.5*mlab.normpdf(x, 30, 10), label='Mixture Distribution')
plt.plot(x,0.3*mlab.normpdf(x, 1, 5), label='First Component')
plt.plot(x,0.2*mlab.normpdf(x, 20, 2),label='Second Component')
plt.plot(x,0.5*mlab.normpdf(x, 30, 10),label='Third Component')
plt.plot([5,5],[0,0.055],'k-',lw=2)
plt.plot([5,59.8],[0.055,0.055],'k-',lw=2)
plt.show()
