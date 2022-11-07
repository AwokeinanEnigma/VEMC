using System;
using System.Runtime.InteropServices;

namespace GpcWrapper
{
	public class GpcWrapper
	{
		public static Tristrip PolygonToTristrip(Polygon polygon)
		{
			GpcWrapper.gpc_tristrip gpc_strip = default(GpcWrapper.gpc_tristrip);
			GpcWrapper.gpc_polygon gpc_pol = GpcWrapper.PolygonTo_gpc_polygon(polygon);
			GpcWrapper.gpc_polygon_to_tristrip(ref gpc_pol, ref gpc_strip);
			Tristrip result = GpcWrapper.gpc_strip_ToTristrip(gpc_strip);
			GpcWrapper.Free_gpc_polygon(gpc_pol);
			GpcWrapper.gpc_free_tristrip(ref gpc_strip);
			return result;
		}
		public static Tristrip ClipToTristrip(GpcOperation operation, Polygon subject_polygon, Polygon clip_polygon)
		{
			GpcWrapper.gpc_tristrip gpc_strip = default(GpcWrapper.gpc_tristrip);
			GpcWrapper.gpc_polygon gpc_pol = GpcWrapper.PolygonTo_gpc_polygon(subject_polygon);
			GpcWrapper.gpc_polygon gpc_pol2 = GpcWrapper.PolygonTo_gpc_polygon(clip_polygon);
			GpcWrapper.gpc_tristrip_clip(operation, ref gpc_pol, ref gpc_pol2, ref gpc_strip);
			Tristrip result = GpcWrapper.gpc_strip_ToTristrip(gpc_strip);
			GpcWrapper.Free_gpc_polygon(gpc_pol);
			GpcWrapper.Free_gpc_polygon(gpc_pol2);
			GpcWrapper.gpc_free_tristrip(ref gpc_strip);
			return result;
		}
		public static Polygon Clip(GpcOperation operation, Polygon subject_polygon, Polygon clip_polygon)
		{
			GpcWrapper.gpc_polygon gpc_polygon = default(GpcWrapper.gpc_polygon);
			GpcWrapper.gpc_polygon gpc_pol = GpcWrapper.PolygonTo_gpc_polygon(subject_polygon);
			GpcWrapper.gpc_polygon gpc_pol2 = GpcWrapper.PolygonTo_gpc_polygon(clip_polygon);
			GpcWrapper.gpc_polygon_clip(operation, ref gpc_pol, ref gpc_pol2, ref gpc_polygon);
			Polygon result = GpcWrapper.gpc_polygon_ToPolygon(gpc_polygon);
			GpcWrapper.Free_gpc_polygon(gpc_pol);
			GpcWrapper.Free_gpc_polygon(gpc_pol2);
			GpcWrapper.gpc_free_polygon(ref gpc_polygon);
			return result;
		}
		public static void SavePolygon(string filename, bool writeHoleFlags, Polygon polygon)
		{
			GpcWrapper.gpc_polygon gpc_pol = GpcWrapper.PolygonTo_gpc_polygon(polygon);
			IntPtr fp = GpcWrapper.fopen(filename, "wb");
			GpcWrapper.gpc_write_polygon(fp, writeHoleFlags ? 1 : 0, ref gpc_pol);
			GpcWrapper.fclose(fp);
			GpcWrapper.Free_gpc_polygon(gpc_pol);
		}
		public static Polygon ReadPolygon(string filename, bool readHoleFlags)
		{
			GpcWrapper.gpc_polygon gpc_polygon = default(GpcWrapper.gpc_polygon);
			IntPtr fp = GpcWrapper.fopen(filename, "rb");
			GpcWrapper.gpc_read_polygon(fp, readHoleFlags ? 1 : 0, ref gpc_polygon);
			Polygon result = GpcWrapper.gpc_polygon_ToPolygon(gpc_polygon);
			GpcWrapper.gpc_free_polygon(ref gpc_polygon);
			GpcWrapper.fclose(fp);
			return result;
		}
		private static GpcWrapper.gpc_polygon PolygonTo_gpc_polygon(Polygon polygon)
		{
			GpcWrapper.gpc_polygon result = default(GpcWrapper.gpc_polygon);
			result.num_contours = polygon.NofContours;
			int[] array = new int[polygon.NofContours];
			for (int i = 0; i < polygon.NofContours; i++)
			{
				array[i] = (polygon.ContourIsHole[i] ? 1 : 0);
			}
			result.hole = Marshal.AllocCoTaskMem(polygon.NofContours * Marshal.SizeOf(array[0]));
			if (polygon.NofContours > 0)
			{
				Marshal.Copy(array, 0, result.hole, polygon.NofContours);
				result.contour = Marshal.AllocCoTaskMem(polygon.NofContours * Marshal.SizeOf(default(GpcWrapper.gpc_vertex_list)));
			}
			IntPtr intPtr = result.contour;
			for (int j = 0; j < polygon.NofContours; j++)
			{
				GpcWrapper.gpc_vertex_list gpc_vertex_list = default(GpcWrapper.gpc_vertex_list);
				gpc_vertex_list.num_vertices = polygon.Contour[j].NofVertices;
				gpc_vertex_list.vertex = Marshal.AllocCoTaskMem(polygon.Contour[j].NofVertices * Marshal.SizeOf(default(GpcWrapper.gpc_vertex)));
				IntPtr intPtr2 = gpc_vertex_list.vertex;
				for (int k = 0; k < polygon.Contour[j].NofVertices; k++)
				{
					GpcWrapper.gpc_vertex gpc_vertex = new GpcWrapper.gpc_vertex
					{
						x = polygon.Contour[j].Vertex[k].X,
						y = polygon.Contour[j].Vertex[k].Y
					};
					Marshal.StructureToPtr(gpc_vertex, intPtr2, false);
					intPtr2 = (IntPtr)((int)intPtr2 + Marshal.SizeOf(gpc_vertex));
				}
				Marshal.StructureToPtr(gpc_vertex_list, intPtr, false);
				intPtr = (IntPtr)((int)intPtr + Marshal.SizeOf(gpc_vertex_list));
			}
			return result;
		}
		private static Polygon gpc_polygon_ToPolygon(GpcWrapper.gpc_polygon gpc_polygon)
		{
			Polygon polygon = new Polygon();
			polygon.NofContours = gpc_polygon.num_contours;
			polygon.ContourIsHole = new bool[polygon.NofContours];
			polygon.Contour = new VertexList[polygon.NofContours];
			int[] array = new int[polygon.NofContours];
			IntPtr intPtr = gpc_polygon.hole;
			if (polygon.NofContours > 0)
			{
				Marshal.Copy(gpc_polygon.hole, array, 0, polygon.NofContours);
			}
			for (int i = 0; i < polygon.NofContours; i++)
			{
				polygon.ContourIsHole[i] = (array[i] != 0);
			}
			intPtr = gpc_polygon.contour;
			for (int j = 0; j < polygon.NofContours; j++)
			{
				GpcWrapper.gpc_vertex_list gpc_vertex_list = (GpcWrapper.gpc_vertex_list)Marshal.PtrToStructure(intPtr, typeof(GpcWrapper.gpc_vertex_list));
				polygon.Contour[j] = new VertexList();
				polygon.Contour[j].NofVertices = gpc_vertex_list.num_vertices;
				polygon.Contour[j].Vertex = new Vertex[polygon.Contour[j].NofVertices];
				IntPtr intPtr2 = gpc_vertex_list.vertex;
				for (int k = 0; k < polygon.Contour[j].NofVertices; k++)
				{
					GpcWrapper.gpc_vertex gpc_vertex = (GpcWrapper.gpc_vertex)Marshal.PtrToStructure(intPtr2, typeof(GpcWrapper.gpc_vertex));
					polygon.Contour[j].Vertex[k].X = gpc_vertex.x;
					polygon.Contour[j].Vertex[k].Y = gpc_vertex.y;
					intPtr2 = (IntPtr)((int)intPtr2 + Marshal.SizeOf(gpc_vertex));
				}
				intPtr = (IntPtr)((int)intPtr + Marshal.SizeOf(gpc_vertex_list));
			}
			return polygon;
		}
		private static Tristrip gpc_strip_ToTristrip(GpcWrapper.gpc_tristrip gpc_strip)
		{
			Tristrip tristrip = new Tristrip();
			tristrip.NofStrips = gpc_strip.num_strips;
			tristrip.Strip = new VertexList[tristrip.NofStrips];
			IntPtr intPtr = gpc_strip.strip;
			for (int i = 0; i < tristrip.NofStrips; i++)
			{
				tristrip.Strip[i] = new VertexList();
				GpcWrapper.gpc_vertex_list gpc_vertex_list = (GpcWrapper.gpc_vertex_list)Marshal.PtrToStructure(intPtr, typeof(GpcWrapper.gpc_vertex_list));
				tristrip.Strip[i].NofVertices = gpc_vertex_list.num_vertices;
				tristrip.Strip[i].Vertex = new Vertex[tristrip.Strip[i].NofVertices];
				IntPtr intPtr2 = gpc_vertex_list.vertex;
				for (int j = 0; j < tristrip.Strip[i].NofVertices; j++)
				{
					GpcWrapper.gpc_vertex gpc_vertex = (GpcWrapper.gpc_vertex)Marshal.PtrToStructure(intPtr2, typeof(GpcWrapper.gpc_vertex));
					tristrip.Strip[i].Vertex[j].X = gpc_vertex.x;
					tristrip.Strip[i].Vertex[j].Y = gpc_vertex.y;
					intPtr2 = (IntPtr)((int)intPtr2 + Marshal.SizeOf(gpc_vertex));
				}
				intPtr = (IntPtr)((int)intPtr + Marshal.SizeOf(gpc_vertex_list));
			}
			return tristrip;
		}
		private static void Free_gpc_polygon(GpcWrapper.gpc_polygon gpc_pol)
		{
			Marshal.FreeCoTaskMem(gpc_pol.hole);
			IntPtr intPtr = gpc_pol.contour;
			for (int i = 0; i < gpc_pol.num_contours; i++)
			{
				GpcWrapper.gpc_vertex_list gpc_vertex_list = (GpcWrapper.gpc_vertex_list)Marshal.PtrToStructure(intPtr, typeof(GpcWrapper.gpc_vertex_list));
				Marshal.FreeCoTaskMem(gpc_vertex_list.vertex);
				intPtr = (IntPtr)((int)intPtr + Marshal.SizeOf(gpc_vertex_list));
			}
			Marshal.FreeCoTaskMem(gpc_pol.contour);
		}
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_polygon_to_tristrip([In] ref GpcWrapper.gpc_polygon polygon, [In] [Out] ref GpcWrapper.gpc_tristrip tristrip);
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_polygon_clip([In] GpcOperation set_operation, [In] ref GpcWrapper.gpc_polygon subject_polygon, [In] ref GpcWrapper.gpc_polygon clip_polygon, [In] [Out] ref GpcWrapper.gpc_polygon result_polygon);
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_tristrip_clip([In] GpcOperation set_operation, [In] ref GpcWrapper.gpc_polygon subject_polygon, [In] ref GpcWrapper.gpc_polygon clip_polygon, [In] [Out] ref GpcWrapper.gpc_tristrip result_tristrip);
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_free_tristrip([In] ref GpcWrapper.gpc_tristrip tristrip);
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_free_polygon([In] ref GpcWrapper.gpc_polygon polygon);
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_read_polygon([In] IntPtr fp, [In] int read_hole_flags, [In] [Out] ref GpcWrapper.gpc_polygon polygon);
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_write_polygon([In] IntPtr fp, [In] int write_hole_flags, [In] ref GpcWrapper.gpc_polygon polygon);
		[DllImport("msvcr71.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr fopen([In] string filename, [In] string mode);
		[DllImport("msvcr71.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void fclose([In] IntPtr fp);
		[DllImport("msvcr71.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int fputc([In] int c, [In] IntPtr fp);
		private enum gpc_op
		{
			GPC_DIFF,
			GPC_INT,
			GPC_XOR,
			GPC_UNION
		}
		private struct gpc_vertex
		{
			public double x;
			public double y;
		}
		private struct gpc_vertex_list
		{
			public int num_vertices;
			public IntPtr vertex;
		}
		private struct gpc_polygon
		{
			public int num_contours;
			public IntPtr hole;
			public IntPtr contour;
		}
		private struct gpc_tristrip
		{
			public int num_strips;
			public IntPtr strip;
		}
	}
}
