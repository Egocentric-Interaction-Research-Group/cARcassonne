import pathlib
from posixpath import basename
import numpy as np
import glob
import os
from PIL import Image
import inspect


from enum import Enum


filename = inspect.getframeinfo(inspect.currentframe()).filename
path = os.path.dirname(os.path.abspath(filename))
class Tile(Enum):
    Cloister = 1 * 25
    Village = 75
    Road = 125
    Grass = 175
    City = 250
    Stream = 6 * 25
    CityStream = 7 * 25 
    RoadStream = 8 * 25
    CityRoad = 9 * 25


filePath = glob.glob(path + "/Assets/PythonImageGenerator/TxtFiles/*.txt")
for x in filePath:
    if os.path.isfile(path +"/Assets/PythonImageGenerator/Images/"+os.path.splitext(x)[0]+".png"):
        continue
    else:
        with open(x,'r+') as f:
            l = []
            for line in f:
                b =[]
                for z in line.split(','):
                    z = z.rstrip("\n")
                    if z == "0":
                        b.append(int(z))
                    else:
                        b.append(int(Tile[z]._value_))
                l.append(b)
    

    input = np.asarray(l)
    

    #plt.imshow(input)
    #plt.axis("off")
    #plt.gray()
    ##print("Images\\"+os.path.splitext(x)[0]+".png")
    #plt.xlim(120)
    #plt.ylim(120)
    #plt.tight_layout()
    #plt.savefig("Images\\"+os.path.splitext(x)[0]+".png")
    img = Image.fromarray(np.uint8(input), 'L')
    #print(os.path.splitext(os.path.basename(x))[0]+".png")
    print(path)
    img.save(path +"/Assets/PythonImageGenerator/Images/"+os.path.splitext(os.path.basename(x))[0]+".png")
    #img.show()
        #l = [ [Tile[x.rstrip('\n')] for x in line.split(',')] for line in f]

#print(l)

#print(input)


    
#input = np.loadtxt(x,dtype="i", delimiter=",")
#print(input)


