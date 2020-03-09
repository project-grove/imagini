using System;
using System.Numerics;
using System.Text;
using Imagini;
using Imagini.Veldrid;
using Veldrid;
using Veldrid.SPIRV;

namespace VeldridGettingStarted
{
	public class Game : VeldridApp
	{
		private CommandList _commandList;
		private DeviceBuffer _vertexBuffer;
		private DeviceBuffer _indexBuffer;
		private Shader[] _shaders;
		private Pipeline _pipeline;

		private const string VertexCode = @"
#version 450
layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;
layout(location = 0) out vec4 fsin_Color;
void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

		private const string FragmentCode = @"
#version 450
layout(location = 0) in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;
void main()
{
    fsout_Color = fsin_Color;
}";

		public Game() : base(new WindowSettings()
		{
			WindowWidth = 960,
			WindowHeight = 540,
			Title = "Veldrid Tutorial",
		})
		{ }

		protected override void Initialize()
		{
			CreateResources();
		}

		protected override void Draw(TimeSpan frameTime)
		{
			// Begin() must be called before commands can be issued.
			_commandList.Begin();

			// We want to render directly to the output window.
			_commandList.SetFramebuffer(Graphics.SwapchainFramebuffer);
			_commandList.ClearColorTarget(0, RgbaFloat.Black);

			// Set all relevant state to draw our quad.
			_commandList.SetVertexBuffer(0, _vertexBuffer);
			_commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
			_commandList.SetPipeline(_pipeline);
			// Issue a Draw command for a single instance with 4 indices.
			_commandList.DrawIndexed(
				indexCount: 4,
				instanceCount: 1,
				indexStart: 0,
				vertexOffset: 0,
				instanceStart: 0);

			// End() must be called before commands can be submitted for execution.
			_commandList.End();
			Graphics.SubmitCommands(_commandList);

			// Once commands have been submitted, the rendered image can be presented to the application window.
			Graphics.SwapBuffers();
		}

		protected override void OnDispose()
		{
			DisposeResources();
		}


		private void CreateResources()
		{
			ResourceFactory factory = Graphics.ResourceFactory;

			VertexPositionColor[] quadVertices =
			{
				new VertexPositionColor(new Vector2(-.75f, .75f), RgbaFloat.Red),
				new VertexPositionColor(new Vector2(.75f, .75f), RgbaFloat.Green),
				new VertexPositionColor(new Vector2(-.75f, -.75f), RgbaFloat.Blue),
				new VertexPositionColor(new Vector2(.75f, -.75f), RgbaFloat.Yellow)
			};
			BufferDescription vbDescription = new BufferDescription(
				4 * VertexPositionColor.SizeInBytes,
				BufferUsage.VertexBuffer);
			_vertexBuffer = factory.CreateBuffer(vbDescription);
			Graphics.UpdateBuffer(_vertexBuffer, 0, quadVertices);

			ushort[] quadIndices = { 0, 1, 2, 3 };
			BufferDescription ibDescription = new BufferDescription(
				4 * sizeof(ushort),
				BufferUsage.IndexBuffer);
			_indexBuffer = factory.CreateBuffer(ibDescription);
			Graphics.UpdateBuffer(_indexBuffer, 0, quadIndices);

			VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
				new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
				new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

			ShaderDescription vertexShaderDesc = new ShaderDescription(
				ShaderStages.Vertex,
				Encoding.UTF8.GetBytes(VertexCode),
				"main");
			ShaderDescription fragmentShaderDesc = new ShaderDescription(
				ShaderStages.Fragment,
				Encoding.UTF8.GetBytes(FragmentCode),
				"main");

			_shaders = factory.CreateFromSpirv(vertexShaderDesc, fragmentShaderDesc);

			// Create pipeline
			GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
			pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;
			pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
				depthTestEnabled: true,
				depthWriteEnabled: true,
				comparisonKind: ComparisonKind.LessEqual);
			pipelineDescription.RasterizerState = new RasterizerStateDescription(
				cullMode: FaceCullMode.Back,
				fillMode: PolygonFillMode.Solid,
				frontFace: FrontFace.Clockwise,
				depthClipEnabled: true,
				scissorTestEnabled: false);
			pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
			pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();
			pipelineDescription.ShaderSet = new ShaderSetDescription(
				vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
				shaders: _shaders);
			pipelineDescription.Outputs = Graphics.SwapchainFramebuffer.OutputDescription;

			_pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

			_commandList = factory.CreateCommandList();
		}

		private void DisposeResources()
		{
			_pipeline.Dispose();
			foreach (Shader shader in _shaders)
			{
				shader.Dispose();
			}
			_commandList.Dispose();
			_vertexBuffer.Dispose();
			_indexBuffer.Dispose();
			Graphics.Dispose();
		}
	}


	struct VertexPositionColor
	{
		public const uint SizeInBytes = 24;
		public Vector2 Position;
		public RgbaFloat Color;
		public VertexPositionColor(Vector2 position, RgbaFloat color)
		{
			Position = position;
			Color = color;
		}
	}
}