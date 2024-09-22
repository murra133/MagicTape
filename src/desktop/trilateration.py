import math as ma
import Rhino.Geometry.Vector3d as vec

e_x = P2-P1
t2 = P3-P1
vec.Unitize(e_x)                     
i = vec.Multiply(e_x,t2)                              
e_y = t2 - i*e_x                                  
vec.Unitize(e_y)
e_z = vec.CrossProduct(e_x, e_y)
t4 = P2-P1
d = t4.Length
j = vec.Multiply(e_y, t2)
x = (r1*r1 - r2*r2 + d*d) / (2*d)                    
y = (r1*r1 - r3*r3 -2*i*x + i*i + j*j) / (2*j)       

t4 = r1*r1 - x*x - y*y                            

if round(t4,4)==0:
    z=0
    Pa = P1 + x*e_x + y*e_y + z*e_z
    Pb = Pa

else:
    if t4<0:
        print("Bad measurement - spheres do not intersect - please try again")
        print("Pa and Pb are null")
    else:
        z = ma.sqrt(t4)
        Pa = P1 + x*e_x + y*e_y + z*e_z                  
        Pb = P1 + x*e_x + y*e_y - z*e_z