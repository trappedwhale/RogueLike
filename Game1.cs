using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace RogueLike
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private Texture2D _playerTexture;
        private Vector2 _playerPosition;
        private Rectangle _sourceRectangle;
        private int _currentFrame;
        private int _totalFrames;
        private float _timer;
        private float _frameTime;

        // 애니메이션 상태
        private enum Direction { Down, Up, Right, Left, IdleDown, IdleUp, IdleRight, IdleLeft }
        private Direction _currentDirection;

        private float _scale;  // 화면 확대/축소 비율

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            IsMouseVisible = true;
            _frameTime = 0.1f; // 각 프레임 간 시간 (애니메이션 속도)

            // 화면 크기 설정 (1280x720 해상도)
            _graphics.PreferredBackBufferWidth = 1280;  // 1280픽셀 (HD 해상도)
            _graphics.PreferredBackBufferHeight = 720;  // 720픽셀
            _graphics.ApplyChanges();

            _scale = 3.0f;  // 3배 확대 (필요에 따라 값을 조정)
        }

        protected override void Initialize()
        {
            _playerPosition = new Vector2(100, 100); // 초기 위치 설정
            _currentDirection = Direction.IdleDown; // 초기 방향: 아래쪽으로 정지 상태
            _currentFrame = 0;
            _totalFrames = 4; // 4개의 프레임 (각 방향에 대해)
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // player.png 이미지 로드 (이미지 경로는 실제 경로로 수정)
            using (var fs = new FileStream("player.png", FileMode.Open))
            {
                _playerTexture = Texture2D.FromStream(GraphicsDevice, fs);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var keyboardState = Keyboard.GetState();
            bool isMoving = false;

            // WASD 키를 사용한 대각선 및 방향 이동 처리
            if (keyboardState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.D))  // 아래 + 오른쪽
            {
                _currentDirection = Direction.Right;
                isMoving = true;
                _playerPosition.X += 2;
                _playerPosition.Y += 2;
            }
            else if (keyboardState.IsKeyDown(Keys.S) && keyboardState.IsKeyDown(Keys.A))  // 아래 + 왼쪽
            {
                _currentDirection = Direction.Left;
                isMoving = true;
                _playerPosition.X -= 2;
                _playerPosition.Y += 2;
            }
            else if (keyboardState.IsKeyDown(Keys.W) && keyboardState.IsKeyDown(Keys.D))  // 위 + 오른쪽
            {
                _currentDirection = Direction.Right;
                isMoving = true;
                _playerPosition.X += 2;
                _playerPosition.Y -= 2;
            }
            else if (keyboardState.IsKeyDown(Keys.W) && keyboardState.IsKeyDown(Keys.A))  // 위 + 왼쪽
            {
                _currentDirection = Direction.Left;
                isMoving = true;
                _playerPosition.X -= 2;
                _playerPosition.Y -= 2;
            }
            else if (keyboardState.IsKeyDown(Keys.S))  // 아래로 이동
            {
                _currentDirection = Direction.Down;
                isMoving = true;
                _playerPosition.Y += 3;
            }
            else if (keyboardState.IsKeyDown(Keys.W))  // 위로 이동
            {
                _currentDirection = Direction.Up;
                isMoving = true;
                _playerPosition.Y -= 3;
            }
            else if (keyboardState.IsKeyDown(Keys.D))  // 오른쪽으로 이동
            {
                _currentDirection = Direction.Right;
                isMoving = true;
                _playerPosition.X += 3;
            }
            else if (keyboardState.IsKeyDown(Keys.A))  // 왼쪽으로 이동
            {
                _currentDirection = Direction.Left;
                isMoving = true;
                _playerPosition.X -= 3;
            }

            // 정지 상태일 때
            if (!isMoving)
            {
                switch (_currentDirection)
                {
                    case Direction.Down:
                        _currentDirection = Direction.IdleDown;
                        break;
                    case Direction.Up:
                        _currentDirection = Direction.IdleUp;
                        break;
                    case Direction.Right:
                        _currentDirection = Direction.IdleRight;
                        break;
                    case Direction.Left:
                        _currentDirection = Direction.IdleLeft;
                        break;
                }
            }

            // 애니메이션 타이머
            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_timer >= _frameTime)
            {
                _timer = 0f;
                _currentFrame = (_currentFrame + 1) % _totalFrames; // 4프레임을 순차적으로 변경
            }

            // 현재 방향에 맞는 소스 사각형 설정
            SetSourceRectangle();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointWrap,  // 픽셀 아트에서 흐림 방지
                DepthStencilState.None,
                RasterizerState.CullCounterClockwise,
                null
            );

            // 스케일링 적용 (확대/축소)
            _spriteBatch.Draw(_playerTexture, _playerPosition * _scale, _sourceRectangle, Color.White, 0f, Vector2.Zero, _scale, SpriteEffects.None, 0f);

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void SetSourceRectangle()
        {
            int frameWidth = _playerTexture.Width / 4;  // 4개의 프레임 (가로)
            int frameHeight = _playerTexture.Height / 4; // 4개의 프레임 (세로)

            switch (_currentDirection)
            {
                case Direction.Down:
                    _sourceRectangle = new Rectangle(_currentFrame * frameWidth, 0 * frameHeight, frameWidth, frameHeight);
                    break;
                case Direction.Up:
                    _sourceRectangle = new Rectangle(_currentFrame * frameWidth, 1 * frameHeight, frameWidth, frameHeight);
                    break;
                case Direction.Right:
                    _sourceRectangle = new Rectangle(_currentFrame * frameWidth, 2 * frameHeight, frameWidth, frameHeight);
                    break;
                case Direction.Left:
                    _sourceRectangle = new Rectangle(_currentFrame * frameWidth, 3 * frameHeight, frameWidth, frameHeight);
                    break;
                case Direction.IdleDown:
                    _sourceRectangle = new Rectangle(0 * frameWidth, 0 * frameHeight, frameWidth, frameHeight);
                    break;
                case Direction.IdleUp:
                    _sourceRectangle = new Rectangle(0 * frameWidth, 1 * frameHeight, frameWidth, frameHeight);
                    break;
                case Direction.IdleRight:
                    _sourceRectangle = new Rectangle(0 * frameWidth, 2 * frameHeight, frameWidth, frameHeight);
                    break;
                case Direction.IdleLeft:
                    _sourceRectangle = new Rectangle(0 * frameWidth, 3 * frameHeight, frameWidth, frameHeight);
                    break;
            }
        }
    }
}
