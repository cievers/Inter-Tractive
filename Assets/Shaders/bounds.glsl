uniform int boundType;
uniform vec3 boundsCenter;
uniform vec3 boundsSize;

uint setBoundFlags(vec3 in_position, uint flags)
{
   if(boundType != 0)
   {
		vec3 bmin = boundsCenter - boundsSize/2.;
		vec3 bmax = boundsCenter + boundsSize/2.;
		bool inBound = in_position.x > bmin.x && in_position.x < bmax.x &&
						  in_position.y > bmin.y && in_position.y < bmax.y &&
						  in_position.z > bmin.z && in_position.z < bmax.z;

		if(boundType == 1 && !inBound)
			 flags = 2;

		if(boundType == 2 && inBound)
			 flags = 2;
   }
   return flags;
}