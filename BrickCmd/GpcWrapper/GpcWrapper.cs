using System;
using System.Runtime.InteropServices;

namespace GpcWrapper
{
	// Token: 0x02000026 RID: 38
	public class GpcWrapper
	{
		// Token: 0x060001D9 RID: 473 RVA: 0x00007FC8 File Offset: 0x000061C8
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

		// Token: 0x060001DA RID: 474 RVA: 0x00008004 File Offset: 0x00006204
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

		// Token: 0x060001DB RID: 475 RVA: 0x00008050 File Offset: 0x00006250
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

		// Token: 0x060001DC RID: 476 RVA: 0x0000809C File Offset: 0x0000629C
		public static void SavePolygon(string filename, bool writeHoleFlags, Polygon polygon)
		{
			GpcWrapper.gpc_polygon gpc_pol = GpcWrapper.PolygonTo_gpc_polygon(polygon);
			IntPtr fp = GpcWrapper.fopen(filename, "wb");
			GpcWrapper.gpc_write_polygon(fp, writeHoleFlags ? 1 : 0, ref gpc_pol);
			GpcWrapper.fclose(fp);
			GpcWrapper.Free_gpc_polygon(gpc_pol);
		}

		// Token: 0x060001DD RID: 477 RVA: 0x000080D8 File Offset: 0x000062D8
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

		// Token: 0x060001DE RID: 478 RVA: 0x00008120 File Offset: 0x00006320
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

		// Token: 0x060001DF RID: 479 RVA: 0x00008308 File Offset: 0x00006508
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

		// Token: 0x060001E0 RID: 480 RVA: 0x000084D4 File Offset: 0x000066D4
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

		// Token: 0x060001E1 RID: 481 RVA: 0x00008630 File Offset: 0x00006830
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

		// Token: 0x060001E2 RID: 482
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_polygon_to_tristrip([In] ref GpcWrapper.gpc_polygon polygon, [In] [Out] ref GpcWrapper.gpc_tristrip tristrip);

		// Token: 0x060001E3 RID: 483
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_polygon_clip([In] GpcOperation set_operation, [In] ref GpcWrapper.gpc_polygon subject_polygon, [In] ref GpcWrapper.gpc_polygon clip_polygon, [In] [Out] ref GpcWrapper.gpc_polygon result_polygon);

		// Token: 0x060001E4 RID: 484
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_tristrip_clip([In] GpcOperation set_operation, [In] ref GpcWrapper.gpc_polygon subject_polygon, [In] ref GpcWrapper.gpc_polygon clip_polygon, [In] [Out] ref GpcWrapper.gpc_tristrip result_tristrip);

		// Token: 0x060001E5 RID: 485
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_free_tristrip([In] ref GpcWrapper.gpc_tristrip tristrip);

		// Token: 0x060001E6 RID: 486
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_free_polygon([In] ref GpcWrapper.gpc_polygon polygon);

		// Token: 0x060001E7 RID: 487
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_read_polygon([In] IntPtr fp, [In] int read_hole_flags, [In] [Out] ref GpcWrapper.gpc_polygon polygon);

		// Token: 0x060001E8 RID: 488
		[DllImport("gpc.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void gpc_write_polygon([In] IntPtr fp, [In] int write_hole_flags, [In] ref GpcWrapper.gpc_polygon polygon);

		// Token: 0x060001E9 RID: 489
		[DllImport("msvcr71.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr fopen([In] string filename, [In] string mode);

		// Token: 0x060001EA RID: 490
		[DllImport("msvcr71.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern void fclose([In] IntPtr fp);

		// Token: 0x060001EB RID: 491
		[DllImport("msvcr71.dll", CallingConvention = CallingConvention.Cdecl)]
		private static extern int fputc([In] int c, [In] IntPtr fp);

		// Token: 0x02000027 RID: 39
		private enum gpc_op
		{
			// Token: 0x0400008B RID: 139
			GPC_DIFF,
			// Token: 0x0400008C RID: 140
			GPC_INT,
			// Token: 0x0400008D RID: 141
			GPC_XOR,
			// Token: 0x0400008E RID: 142
			GPC_UNION
		}

		// Token: 0x02000028 RID: 40
		private struct gpc_vertex
		{
			// Token: 0x0400008F RID: 143
			public double x;

			// Token: 0x04000090 RID: 144
			public double y;
		}

		// Token: 0x02000029 RID: 41
		private struct gpc_vertex_list
		{
			// Token: 0x04000091 RID: 145
			public int num_vertices;

			// Token: 0x04000092 RID: 146
			public IntPtr vertex;
		}

		// Token: 0x0200002A RID: 42
		private struct gpc_polygon
		{
			// Token: 0x04000093 RID: 147
			public int num_contours;

			// Token: 0x04000094 RID: 148
			public IntPtr hole;

			// Token: 0x04000095 RID: 149
			public IntPtr contour;
		}

		// Token: 0x0200002B RID: 43
		private struct gpc_tristrip
		{
			// Token: 0x04000096 RID: 150
			public int num_strips;

			// Token: 0x04000097 RID: 151
			public IntPtr strip;
		}
	}
}
