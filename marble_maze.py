from direct.showbase.ShowBase import ShowBase
from direct.gui.OnscreenText import OnscreenText
from panda3d.core import *
from direct.task import Task


class MarbleMaze(ShowBase):

    def __init__(self):
        ShowBase.__init__(self)

        self.disableMouse()

        self.accept("escape", exit)
        self.accept("r", self.reset)

        self.keys = {
            "w": False,
            "a": False,
            "s": False,
            "d": False
        }

        for k in self.keys:
            self.accept(k, self.setKey, [k, True])
            self.accept(k + "-up", self.setKey, [k, False])

        self.setupLights()
        self.createLevel()

        self.taskMgr.add(self.update, "update")

    def setupLights(self):
        ambient = AmbientLight("ambient")
        ambient.setColor((0.6, 0.6, 0.6, 1))
        self.render.setLight(self.render.attachNewNode(ambient))

        sun = DirectionalLight("sun")
        sun.setColor((1, 1, 1, 1))
        sun_np = self.render.attachNewNode(sun)
        sun_np.setHpr(-45, -45, 0)
        self.render.setLight(sun_np)

    def apply_solid_color(self, model, r, g, b, a=1):
        """Disable textures so setColor produces a flat solid color."""
        model.setTextureOff(TextureStage.getDefault())
        model.setColor(r, g, b, a)

    def make_wall(self, x, y, sx, sy):
        wall = self.loader.loadModel("models/box")
        wall.reparentTo(self.render)
        wall.setScale(sx, sy, 1)
        wall.setPos(x, y, 1)
        self.apply_solid_color(wall, 0.45, 0.45, 0.45)
        return wall

    def createLevel(self):
        self.floor = self.loader.loadModel("models/box")
        self.floor.reparentTo(self.render)
        self.floor.setScale(20, 20, 0.5)
        self.floor.setPos(0, 0, 0)
        self.apply_solid_color(self.floor, 0.4, 0.7, 0.4)

        BOARD = 20
        self.walls = []

        # Outer boundary
        self.walls.append(self.make_wall(0, BOARD, BOARD, 0.5))
        self.walls.append(self.make_wall(0, -BOARD, BOARD, 0.5))
        self.walls.append(self.make_wall(BOARD, 0, 0.5, BOARD))
        self.walls.append(self.make_wall(-BOARD, 0, 0.5, BOARD))

        # Inner boundary
        self.walls.append(self.make_wall(0, 15, 15, 0.4))
        self.walls.append(self.make_wall(0, -15, 15, 0.4))
        self.walls.append(self.make_wall(15, 0, 0.4, 15))
        self.walls.append(self.make_wall(-15, 0, 0.4, 15))

        # Maze walls
        self.walls.append(self.make_wall(-5, 5, 0.4, 6))
        self.walls.append(self.make_wall(5, -4, 0.4, 6))
        self.walls.append(self.make_wall(0, 0, 5, 0.4))
        self.walls.append(self.make_wall(-8, -6, 4, 0.4))
        self.walls.append(self.make_wall(8, 6, 4, 0.4))
        self.walls.append(self.make_wall(0, 0, 8, 0.5))
        self.walls.append(self.make_wall(-8, 6, 0.5, 8))
        self.walls.append(self.make_wall(8, -6, 0.5, 8))
        self.walls.append(self.make_wall(6, 10, 6, 0.5))
        self.walls.append(self.make_wall(-6, -10, 6, 0.5))

        self.ball = self.loader.loadModel("models/smiley")
        self.ball.reparentTo(self.render)
        self.ball.setScale(0.6)
        self.ball.setPos(-12, -12, 1)

        self.goal = self.loader.loadModel("models/box")
        self.goal.reparentTo(self.render)
        self.goal.setScale(1)
        self.goal.setPos(18, 18, 1)
        self.apply_solid_color(self.goal, 1, 1, 0)

        self.text = OnscreenText(
            text="Reach the yellow cube!  WASD = move  R = restart",
            pos=(-1.25, 0.9),
            scale=0.055,
            align=TextNode.ALeft
        )

        self.camera.setPos(0, -35, 25)
        self.camera.lookAt(0, 0, 0)

    def setKey(self, key, value):
        self.keys[key] = value

    def reset(self):
        self.ball.setPos(-12, -12, 1)
        self.text.setText("Reach the yellow cube!  WASD = move  R = restart")

    def hits_wall(self, pos):
        for wall in self.walls:
            wp = wall.getPos()
            sx = wall.getScale().x
            sy = wall.getScale().y

            if abs(pos.x - wp.x) < sx + 0.7 and abs(pos.y - wp.y) < sy + 0.7:
                return True

        return False

    def update(self, task):
        dt = globalClock.getDt()

        old_pos = self.ball.getPos()
        new_pos = old_pos

        speed = 8

        if self.keys["w"]:
            new_pos.y += speed * dt

        if self.keys["s"]:
            new_pos.y -= speed * dt

        if self.keys["a"]:
            new_pos.x -= speed * dt

        if self.keys["d"]:
            new_pos.x += speed * dt

        new_pos.x = max(-19, min(19, new_pos.x))
        new_pos.y = max(-19, min(19, new_pos.y))
        if not self.hits_wall(new_pos):
            self.ball.setPos(new_pos)
        else:
            self.ball.setPos(old_pos)

        pos = self.ball.getPos()

        self.camera.setPos(pos.x, pos.y - 24, pos.z + 14)
        self.camera.lookAt(self.ball)

        if (self.goal.getPos() - pos).length() < 1.5:
            self.text.setText("YOU WIN! Press R to play again")

        return Task.cont


game = MarbleMaze()
game.run()
