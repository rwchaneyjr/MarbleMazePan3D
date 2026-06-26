from direct.showbase.ShowBase import ShowBase
from direct.gui.OnscreenText import OnscreenText
from panda3d.core import *
from direct.task import Task
import math


# Square outer wall; entrance/exit gaps at top & bottom center (cols 4-5)
MAZE_LAYOUT = [
    "#### ####",
    "#  #S   #",
    "#  # ## #",
    "#    #  #",
    "# ####  #",
    "# #     #",
    "# # #####",
    "# #     #",
    "#   G## #",
    "#### ####",
]


class MarbleMaze(ShowBase):

    CELL_SIZE = 3
    WALL_THICK = 0.12
    FLOOR_PADDING = 2

    BALL_SCALE = 0.32
    BALL_RADIUS = BALL_SCALE * 0.5
    HIT_RADIUS = BALL_RADIUS + 0.05

    FLOOR_THICK = 0.2
    WALL_HEIGHT = 0.6
    FLOOR_Z = 0
    FLOOR_TOP = FLOOR_Z + FLOOR_THICK / 2
    WALL_Z = FLOOR_TOP + WALL_HEIGHT / 2
    BALL_Z = FLOOR_TOP + BALL_RADIUS

    FLOOR_COLOR = (0.97, 0.97, 0.97, 1)
    WALL_COLOR = (1.0, 1.0, 1.0, 1)

    def __init__(self):
        ShowBase.__init__(self)

        self.disableMouse()
        self.setBackgroundColor(0.97, 0.97, 0.97, 1)

        self.accept("escape", exit)
        self.accept("r", self.reset)

        self.keys = {"w": False, "a": False, "s": False, "d": False}
        for k in self.keys:
            self.accept(k, self.setKey, [k, True])
            self.accept(k + "-up", self.setKey, [k, False])

        self.setupLights()
        self.createLevel()
        self.taskMgr.add(self.update, "update")

    def setupLights(self):
        ambient = AmbientLight("ambient")
        ambient.setColor((0.85, 0.85, 0.85, 1))
        self.render.setLight(self.render.attachNewNode(ambient))

        sun = DirectionalLight("sun")
        sun.setColor((1, 1, 1, 1))
        sun_np = self.render.attachNewNode(sun)
        sun_np.setHpr(-45, -55, 0)
        self.render.setLight(sun_np)

    def apply_solid_color(self, model, r, g, b, a=1):
        model.setTextureOff(TextureStage.getDefault())
        model.setColor(r, g, b, a)

    def cell_center(self, col, row, cols, rows):
        x = -cols * self.CELL_SIZE / 2 + col * self.CELL_SIZE + self.CELL_SIZE / 2
        y = rows * self.CELL_SIZE / 2 - row * self.CELL_SIZE - self.CELL_SIZE / 2
        return x, y

    def layout_is_wall(self, col, row):
        if col < 0 or col >= self.maze_cols or row < 0 or row >= self.maze_rows:
            return False
        return MAZE_LAYOUT[row][col] == "#"

    def add_wall_edge(self, x, y, sx, sy):
        self.walls.append(self.make_wall(x, y, sx, sy))
        self.wall_boxes.append((x, y, sx, sy))

    def wall_edges_for_cell(self, col, row, x, y):
        """Draw thin edges only where wall meets open space — uniform thickness."""
        half = self.CELL_SIZE / 2
        span = self.CELL_SIZE + self.WALL_THICK

        if not self.layout_is_wall(col, row - 1):
            self.add_wall_edge(x, y + half, span, self.WALL_THICK)
        if not self.layout_is_wall(col, row + 1):
            self.add_wall_edge(x, y - half, span, self.WALL_THICK)
        if not self.layout_is_wall(col + 1, row):
            self.add_wall_edge(x + half, y, self.WALL_THICK, span)
        if not self.layout_is_wall(col - 1, row):
            self.add_wall_edge(x - half, y, self.WALL_THICK, span)

    def make_wall(self, x, y, sx, sy):
        wall = self.loader.loadModel("models/box")
        wall.reparentTo(self.render)
        wall.setScale(sx, sy, self.WALL_HEIGHT)
        wall.setPos(x, y, self.WALL_Z)
        self.apply_solid_color(wall, *self.WALL_COLOR)
        return wall

    def hits_wall(self, x, y):
        r = self.HIT_RADIUS
        for wx, wy, sx, sy in self.wall_boxes:
            hw = sx * 0.5
            hh = sy * 0.5
            closest_x = max(wx - hw, min(x, wx + hw))
            closest_y = max(wy - hh, min(y, wy + hh))
            dx = x - closest_x
            dy = y - closest_y
            if dx * dx + dy * dy < r * r:
                return True
        return False

    def build_maze(self, layout):
        self.walls = []
        self.wall_boxes = []
        start_pos = None
        goal_pos = None

        rows = len(layout)
        cols = len(layout[0])

        for row, line in enumerate(layout):
            for col, ch in enumerate(line):
                x, y = self.cell_center(col, row, cols, rows)

                if ch == "#":
                    self.wall_edges_for_cell(col, row, x, y)
                elif ch == "S":
                    start_pos = (x, y)
                elif ch == "G":
                    goal_pos = (x, y)

        return start_pos, goal_pos, cols, rows

    def createLevel(self):
        self.maze_cols = len(MAZE_LAYOUT[0])
        self.maze_rows = len(MAZE_LAYOUT)

        start_pos, goal_pos, cols, rows = self.build_maze(MAZE_LAYOUT)
        self.start_pos = start_pos

        span_x = cols * self.CELL_SIZE + self.FLOOR_PADDING
        span_y = rows * self.CELL_SIZE + self.FLOOR_PADDING
        self.play_limit = cols * self.CELL_SIZE / 2 - self.HIT_RADIUS - 0.2

        self.floor = self.loader.loadModel("models/box")
        self.floor.reparentTo(self.render)
        self.floor.setScale(span_x, span_y, self.FLOOR_THICK)
        self.floor.setPos(0, 0, self.FLOOR_Z)
        self.apply_solid_color(self.floor, *self.FLOOR_COLOR)

        self.ball = self.loader.loadModel("models/smiley")
        self.ball.reparentTo(self.render)
        self.ball.setScale(self.BALL_SCALE)
        self.ball.setPos(start_pos[0], start_pos[1], self.BALL_Z)

        self.goal = self.loader.loadModel("models/box")
        self.goal.reparentTo(self.render)
        self.goal.setScale(0.45)
        self.goal.setPos(goal_pos[0], goal_pos[1], self.BALL_Z)
        self.apply_solid_color(self.goal, 1.0, 0.85, 0.1)

        self.text = OnscreenText(
            text="Enter top, exit bottom!  WASD = move  R = restart",
            pos=(-1.25, 0.9),
            scale=0.055,
            align=TextNode.ALeft,
            fg=(0.1, 0.1, 0.1, 1),
        )

        # Top-down view like the photo
        self.camera.setPos(0, 0, span_y * 1.35)
        self.camera.lookAt(0, 0, 0)

    def setKey(self, key, value):
        self.keys[key] = value

    def reset(self):
        self.ball.setPos(self.start_pos[0], self.start_pos[1], self.BALL_Z)
        self.text.setText("Enter top, exit bottom!  WASD = move  R = restart")

    def try_move(self, x, y, dx, dy):
        steps = max(1, int(math.ceil(max(abs(dx), abs(dy)) / 0.1)))

        for _ in range(steps):
            step_x = dx / steps
            step_y = dy / steps
            new_x = x + step_x
            new_y = y + step_y

            if not self.hits_wall(new_x, new_y):
                x, y = new_x, new_y
                continue

            moved = False
            if not self.hits_wall(x + step_x, y):
                x += step_x
                moved = True
            if not self.hits_wall(x, y + step_y):
                y += step_y
                moved = True
            if not moved:
                break

        x = max(-self.play_limit, min(self.play_limit, x))
        y = max(-self.play_limit, min(self.play_limit, y))
        return x, y

    def update(self, task):
        dt = globalClock.getDt()
        x = self.ball.getPos().x
        y = self.ball.getPos().y
        speed = 7
        dx = dy = 0.0

        if self.keys["w"]:
            dy += speed * dt
        if self.keys["s"]:
            dy -= speed * dt
        if self.keys["a"]:
            dx -= speed * dt
        if self.keys["d"]:
            dx += speed * dt

        if dx != 0.0 or dy != 0.0:
            x, y = self.try_move(x, y, dx, dy)
            self.ball.setPos(x, y, self.BALL_Z)

        if (self.goal.getPos() - self.ball.getPos()).length() < 1.2:
            self.text.setText("YOU REACHED THE EXIT! Press R to play again")

        return Task.cont


game = MarbleMaze()
game.run()
